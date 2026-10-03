/*
 * File:        CreateReservationRequest.java
 * Author:      Dissanayake D.M.S.N (IT22210692)
 * Description: Request model used to create an energy reservation
 *              through the Web API.
 * Created:     30/09/2026
 */

package com.solargrid.app.models;

/**
 * Represents the data sent to the Web API when creating an
 * energy reservation.
 */
public class CreateReservationRequest {

    private String prosumerNic;
    private String stationId;
    private String slotId;
    private String reservationTime;
    private String type;
    private double energyKwh;

    /**
     * Creates a reservation request with the selected prosumer,
     * station, booking slot, reservation time, reservation type
     * and requested energy amount.
     *
     * @param prosumerNic NIC of the prosumer making the reservation
     * @param stationId ID of the selected solar station
     * @param slotId ID of the selected booking slot
     * @param reservationTime selected reservation date and time
     * @param type reservation type
     * @param energyKwh requested energy amount in kWh
     */
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

    /**
     * Returns the NIC of the prosumer making the reservation.
     *
     * @return prosumer NIC
     */
    public String getProsumerNic() {
        return prosumerNic;
    }

    /**
     * Returns the selected solar station ID.
     *
     * @return station ID
     */
    public String getStationId() {
        return stationId;
    }

    /**
     * Returns the selected booking slot ID.
     *
     * @return slot ID
     */
    public String getSlotId() {
        return slotId;
    }

    /**
     * Returns the selected reservation date and time.
     *
     * @return reservation date and time
     */
    public String getReservationTime() {
        return reservationTime;
    }

    /**
     * Returns the selected reservation type.
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
}