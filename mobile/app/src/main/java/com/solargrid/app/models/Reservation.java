/*
 * File:        Reservation.java
 * Author:      Dissanayake D.M.S.N (IT22210692)
 * Description: Model representing an energy reservation returned by the Web API.
 * Created:     29/09/2026
 */

package com.solargrid.app.models;

import com.google.gson.annotations.SerializedName;

public class Reservation {

    private String id;

    private String prosumerNic;

    private String stationId;

    private String stationName;

    private String slotId;

    @SerializedName("reservationTime")
    private String reservationTime;

    private String type;

    private double energyKwh;

    private String status;

    private String qrToken;

    public Reservation() {
    }

    public String getId() {
        return id;
    }

    public String getProsumerNic() {
        return prosumerNic;
    }

    public String getStationId() {
        return stationId;
    }

    public String getStationName() {
        return stationName;
    }

    public String getSlotId() {
        return slotId;
    }

    public String getReservationTime() {
        return reservationTime;
    }

    public String getType() {
        return type;
    }

    public double getEnergyKwh() {
        return energyKwh;
    }

    public String getStatus() {
        return status;
    }

    public String getQrToken() {
        return qrToken;
    }
}