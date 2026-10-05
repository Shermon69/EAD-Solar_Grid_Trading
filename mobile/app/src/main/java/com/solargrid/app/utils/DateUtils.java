/*
 * File:        DateUtils.java
 * Author:      Shermon H (IT22177964)
 * Description: Date helpers. The Web API sends all times in UTC; this turns
 *              them into the phone's local time for display.
 */

package com.solargrid.app.utils;

import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;
import java.util.TimeZone;

/**
 * Helper methods for showing API dates and times.
 */
public final class DateUtils {

    /**
     * Private constructor: this class only has static methods.
     */
    private DateUtils() {
    }

    /**
     * Converts an API timestamp such as 2026-10-03T04:30:00Z (UTC) to the phone's
     * local date and time, e.g. "03 Oct 2026, 10:00 AM". If the text cannot be read,
     * it is returned unchanged so the screen still shows something.
     */
    public static String formatLocal(String utcText) {
        return convert(utcText, "dd MMM yyyy, hh:mm a");
    }

    /**
     * Same conversion, but only the local date, e.g. "03 Oct 2026".
     */
    public static String formatLocalDate(String utcText) {
        return convert(utcText, "dd MMM yyyy");
    }

    /**
     * Same conversion, but only the local time, e.g. "10:00 AM".
     */
    public static String formatLocalTime(String utcText) {
        return convert(utcText, "hh:mm a");
    }

    /**
     * Reads an API UTC timestamp and writes it in the phone's time zone using the
     * given pattern. Returns the original text if it cannot be read.
     */
    private static String convert(String utcText, String outputPattern) {
        if (utcText == null) {
            return "";
        }
        if (utcText.length() < 19) {
            return utcText;
        }

        try {
            // Keep only yyyy-MM-ddTHH:mm:ss (drops fractions of a second and the "Z")
            SimpleDateFormat input = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.ENGLISH);
            input.setTimeZone(TimeZone.getTimeZone("UTC"));
            Date date = input.parse(utcText.substring(0, 19));

            SimpleDateFormat output = new SimpleDateFormat(outputPattern, Locale.ENGLISH);
            output.setTimeZone(TimeZone.getDefault());

            return date == null ? utcText : output.format(date);
        } catch (ParseException e) {
            return utcText;
        }
    }
}
