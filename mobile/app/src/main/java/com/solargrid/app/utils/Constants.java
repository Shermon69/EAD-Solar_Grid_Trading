/*
 * File:        Constants.java
 * Author:      Shermon H (IT22177964)
 * Description: Fixed values used in the app. Role names must match the Web API.
 * Created:     29/09/2026
 */

package com.solargrid.app.utils;

/**
 * App-wide constant values.
 */
public final class Constants {

    // User roles (same values as the API)
    public static final String ROLE_PROSUMER = "Prosumer";
    public static final String ROLE_GRID_OPERATOR = "GridOperator";
    public static final String ROLE_BACKOFFICE = "Backoffice";

    // Reservation statuses (same values as the API)
    public static final String STATUS_PENDING = "Pending";
    public static final String STATUS_APPROVED = "Approved";
    public static final String STATUS_COMPLETED = "Completed";
    public static final String STATUS_CANCELLED = "Cancelled";

    // Key for passing a reservation ID between screens with Intent extras
    public static final String EXTRA_RESERVATION_ID = "reservation_id";
    public static final String EXTRA_STATION_ID = "station_id";

    /**
     * Private constructor: this class only holds constants.
     */
    private Constants() {
    }
}
