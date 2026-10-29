package com.northwindpay.fxrates;

/**
 * Entry point for the nightly FX batch.
 */
public class FxBatch {

    public static void main(String[] args) {
        RateCalculator calculator = new RateCalculator();
        calculator.load("EUR,USD,1.08\nUSD,EUR,0.92\nEUR,GBP,0.85");
        System.out.println(calculator.convert("EUR", "USD", 100, true, false, 2));
        System.out.println(calculator.report());
    }
}
