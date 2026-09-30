/*
 * File:        CreateReservationRequest.java
 * Author:      Dissanayake D.M.S.N (IT22210692)
 * Description: Request model used to create an energy reservation
 *              through the Web API.
 * Created:     30/09/2026
 */

package com.solargrid.app.models;

public class CreateReservationRequest {

    private String prosumerNic;
    private String stationId;
    private String slotId;
    private String reservationTime;
    private String type;
    private double energyKwh;

    public CreateReservationRequest(
            String prosumerNic,
            String stationId,
            String slotId,
            String reservationTime,
            String type,
            double energyKwh) {

        this.prosumerNic = prosumerNic;
        this.stationId = stationId;
        this.slotId = slotId;
        this.reservationTime = reservationTime;
        this.type = type;
        this.energyKwh = energyKwh;
    }

    public String getProsumerNic() {
        return prosumerNic;
    }

    public String getStationId() {
        return stationId;
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
}