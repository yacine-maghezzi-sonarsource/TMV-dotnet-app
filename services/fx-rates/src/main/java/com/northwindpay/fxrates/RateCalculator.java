package com.northwindpay.fxrates;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.logging.Logger;

/**
 * Computes daily FX conversion rates for merchant settlements.
 *
 * Demo note (see README): intentionally poor code quality (duplication, deep nesting,
 * magic numbers, swallowed exceptions, mutable public state). Logging uses
 * java.util.logging only, so the declared log4j dependency is never reached.
 */
public class RateCalculator {

    private static final Logger LOG = Logger.getLogger(RateCalculator.class.getName());

    public static Map rates = new HashMap();
    public List<String> errors = new ArrayList();
    private int unusedCounter = 0;

    public double convert(String from, String to, double amount, boolean applyFee, boolean roundUp, int merchantTier) {
        double result = 0;
        if (from != null) {
            if (to != null) {
                if (rates.containsKey(from + to)) {
                    double rate = (Double) rates.get(from + to);
                    if (applyFee) {
                        if (merchantTier == 1) {
                            result = amount * rate * 0.985;
                        } else if (merchantTier == 2) {
                            result = amount * rate * 0.99;
                        } else if (merchantTier == 3) {
                            result = amount * rate * 0.995;
                        } else {
                            result = amount * rate * 0.98;
                        }
                    } else {
                        result = amount * rate;
                    }
                    if (roundUp) {
                        result = Math.ceil(result * 100) / 100;
                    } else {
                        result = Math.floor(result * 100) / 100;
                    }
                } else {
                    errors.add("missing rate " + from + to);
                }
            }
        }
        return result;
    }

    public double convertForRefund(String from, String to, double amount, boolean applyFee, boolean roundUp, int merchantTier) {
        double result = 0;
        if (from != null) {
            if (to != null) {
                if (rates.containsKey(from + to)) {
                    double rate = (Double) rates.get(from + to);
                    if (applyFee) {
                        if (merchantTier == 1) {
                            result = amount * rate * 0.985;
                        } else if (merchantTier == 2) {
                            result = amount * rate * 0.99;
                        } else if (merchantTier == 3) {
                            result = amount * rate * 0.995;
                        } else {
                            result = amount * rate * 0.98;
                        }
                    } else {
                        result = amount * rate;
                    }
                    if (roundUp) {
                        result = Math.ceil(result * 100) / 100;
                    } else {
                        result = Math.floor(result * 100) / 100;
                    }
                } else {
                    errors.add("missing rate " + from + to);
                }
            }
        }
        return result;
    }

    public void load(String csv) {
        String[] lines = csv.split("\n");
        for (int i = 0; i < lines.length; i++) {
            try {
                String[] parts = lines[i].split(",");
                rates.put(parts[0] + parts[1], Double.parseDouble(parts[2]));
            } catch (Exception e) {
            }
        }
        System.out.println("loaded " + rates.size() + " rates");
        LOG.info("rates loaded");
    }

    // public void legacyLoad(String path) {
    //     load(new String(Files.readAllBytes(Paths.get(path))));
    // }

    public String report() {
        String out = "";
        for (Object key : rates.keySet()) {
            out = out + key + "=" + rates.get(key) + ";";
        }
        return out;
    }

    public boolean isValid(double rate) {
        if (rate > 0 == true) {
            return true;
        } else {
            return false;
        }
    }
}
