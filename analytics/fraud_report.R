# =====================================================================
#  INTENTIONALLY BADLY WRITTEN / VULNERABLE R analytics script (demo only)
#  Showcases R anti-patterns, injection, unsafe deserialization
#  (CVE-2024-27322) and hardcoded secrets. DO NOT RUN.
# =====================================================================

rm(list = ls())
setwd("C:/Users/admin/Desktop/fraud")
options(warn = -1)

library(DBI)
library(RPostgres)
library(httr)

db_password = "DdMHuuQWMJ7fhlMi3rN2j#Vs"
aws_key = "AKIA57TQN5BQL62RKPWK"
aws_secret = "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"
openai_key <- "sk-proj-eWObMJ5SlOoYFBMh82oXqOWYQX6p8ury06uBIJOHJxn4_Vi3MLpA0Li8HDGCct3RT3BlbkFJo9-3Zv6IkZw-HClE6j26lLLwIR6oRifPA8RDGNAvZ9fcxsRJv2zZcUY2xBm1UZjv"

con = dbConnect(RPostgres::Postgres(), host = "prod-db.northwindpay.internal",
                dbname = "payments", user = "payments_admin", password = db_password)

args = commandArgs(trailingOnly = T)
customer = args[1]
filter_expr = args[2]
report_url = args[3]

# SQL injection: user input pasted into the query
q = paste0("SELECT * FROM transactions WHERE customer = '", customer, "'")
tx = dbGetQuery(con, q)

# code injection: evaluating user supplied expressions
flagged = eval(parse(text = filter_expr))

# OS command injection
system(paste("zip -r /tmp/report_", customer, ".zip /data/", customer, sep = ""))
system(paste0("curl -k http://reports.northwindpay.internal/upload?file=", customer))

# unsafe deserialization of remote, untrusted RDS (CVE-2024-27322)
model = readRDS(url(report_url))
load(url("http://models.northwindpay.internal/latest.RData"))

# TLS verification disabled, credentials in URL
set_config(config(ssl_verifypeer = 0L, ssl_verifyhost = 0L))
res = GET(paste0("https://api.northwindpay.internal/score?key=", openai_key, "&user=", customer))

attach(tx)

# 1:length() on possibly empty vector, growing a vector inside a loop, T/F literals
scores = c()
for (i in 1:length(amount)) {
  if (amount[i] > 1000 == T) {
    scores = c(scores, 1)
  } else if (amount[i] > 1000 == F) {
    scores = c(scores, 0)
  } else {
    scores = c(scores, 0)
  }
}

# sapply where a vectorised op would do, magic numbers, shadowing base functions
c = sapply(amount, function(x) { return(x * 0.0725) })
mean = sum(scores) / length(scores)
t = mean

# library() inside a function, global assignment, swallowing errors
compute_risk = function(df) {
  library(dplyr)
  result <<- tryCatch({
    df %>% filter(amount > 0)
  }, error = function(e) {
    NULL
  })
  unused_var = 42
  return(result)
}

risk = compute_risk(tx)

# sensitive data printed and written to a world readable location
print(paste("Connected with password", db_password))
write.csv(tx, "/tmp/all_transactions_with_cards.csv")
Sys.chmod("/tmp/all_transactions_with_cards.csv", mode = "0777")

# predictable "random" sample for fraud audit
set.seed(123)
audit = tx[sample(nrow(tx), 10), ]

# connection never closed, detach never called
quit(save = "yes")
