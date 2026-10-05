/*
 * File:        Reservation.java
 * Author:      Dissanayake D.M.S.N (IT22210692)
 * Description: Model representing an energy reservation returned by the Web API.
 */

package com.solargrid.app.models;

import com.google.gson.annotations.SerializedName;

/**
 * Represents an energy reservation returned by the central Web API.
 *
 * The model contains the reservation details used by the mobile
 * reservation list, details, edit and cancellation screens.
 */
public class Reservation {

    private String id;

    private String prosumerNic;

    private String stationId;

    private String stationName;

    private String slotId;

    /*
     * Maps the JSON property "reservationTime" returned by the
     * Web API to the reservationTime field used by the Android app.
     */
    @SerializedName("reservationTime")
    private String reservationTime;

    private String type;

    private double energyKwh;

    private String status;

    private String qrToken;

    /**
     * Creates an empty reservation object for Gson deserialization.
     */
    public Reservation() {
    }

    /**
     * Returns the unique reservation ID.
     *
     * @return reservation ID
     */
    public String getId() {
        return id;
    }

    /**
     * Returns the NIC of the prosumer who created the reservation.
     *
     * @return prosumer NIC
     */
    public String getProsumerNic() {
        return prosumerNic;
    }

    /**
     * Returns the ID of the solar station selected for the reservation.
     *
     * @return station ID
     */
    public String getStationId() {
        return stationId;
    }

    /**
     * Returns the name of the selected solar station.
     *
     * @return station name
     */
    public String getStationName() {
        return stationName;
    }

    /**
     * Returns the ID of the booking slot selected for the reservation.
     *
     * @return slot ID
     */
    public String getSlotId() {
        return slotId;
    }

    /**
     * Returns the reservation date and time.
     *
     * @return reservation date and time
     */
    public String getReservationTime() {
        return reservationTime;
    }

    /**
     * Returns the reservation type.
     *
     * @return reservation type
     */
    public String getType() {
        return type;
    }

    /**
     * Returns the requested energy amount.
     *
     * @return energy amount in kWh
     */
    public double getEnergyKwh() {
        return energyKwh;
    }

    /**
     * Returns the current reservation status.
     *
     * @return reservation status
     */
    public String getStatus() {
        return status;
    }

    /**
     * Returns the QR token generated for an approved reservation.
     *
     * @return QR token, or null when one has not been generated
     */
    public String getQrToken() {
        return qrToken;
    }
}