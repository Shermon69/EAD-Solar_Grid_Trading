/*
 * File:        Booking.java
 * Author:      Premaratne R.A.N.C (IT22050908)
 * Description: One booking row returned by the API. Used in the bookings
 *              list and dashboard counts. Mapped from JSON by Gson.
 */
package com.solargrid.app.models;

/**
 * A single booking (reservation) returned by the API. Matches the
 * {@code BookingResponse} DTO on the server side.
 */
public class Booking {

    /** Booking reference (the reservation's _id). */
    public String id;

    /** Name of the station where the booking was made. */
    public String stationName;

    /** Start time of the booked slot (ISO-8601 string, UTC). */
    public String reservationDate;

    /** One of: Pending, Approved, Completed, Cancelled. */
    public String status;

    /** QR token, only set when the booking is Approved. Null otherwise. */
    public String qrToken;
}