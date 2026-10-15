/*
 * File:        OperatingHours.java
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: Model for one day of a solar station's weekly opening hours,
 *              e.g. Monday 06:00 - 18:00, as returned by the Web API.
 * Created:     15/10/2026
 */

package com.solargrid.app.models;

import java.io.Serializable;

/**
 * Opening hours of a station for one day of the week.
 */
public class OperatingHours implements Serializable {

    private static final long serialVersionUID = 1L;

    private String day;
    private String openTime;
    private String closeTime;

    /**
     * Returns the day name, e.g. "Monday".
     *
     * @return day of the week
     */
    public String getDay() {
        return day;
    }

    /**
     * Returns the opening time in HH:mm format.
     *
     * @return opening time
     */
    public String getOpenTime() {
        return openTime;
    }

    /**
     * Returns the closing time in HH:mm format.
     *
     * @return closing time
     */
    public String getCloseTime() {
        return closeTime;
    }
}
