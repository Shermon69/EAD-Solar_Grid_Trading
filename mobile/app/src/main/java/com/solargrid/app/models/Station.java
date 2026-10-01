/*
 * File:        Station.java
 * Author:      Dissanayake D.M.S.N (IT22210692)
 * Description: Model representing a solar microgrid station returned
 *              by the Web API.
 * Created:     30/09/2026
 */

package com.solargrid.app.models;

import com.google.gson.annotations.SerializedName;

import java.io.Serializable;

/**
 * Represents a solar microgrid station.
 */
public class Station implements Serializable {

    private static final long serialVersionUID = 1L;

    private String id;
    private String name;
    private String address;

    private double latitude;
    private double longitude;

    @SerializedName("capacityKw")
    private double capacityKw;

    private int batterySlots;
    private boolean isActive;

    public Station() {
    }

    /**
     * Returns the station ID.
     *
     * @return station ID
     */
    public String getId() {
        return id;
    }

    /**
     * Returns the station name.
     *
     * @return station name
     */
    public String getName() {
        return name;
    }

    /**
     * Returns the station address.
     *
     * @return station address
     */
    public String getAddress() {
        return address;
    }

    /**
     * Returns the station latitude.
     *
     * @return latitude
     */
    public double getLatitude() {
        return latitude;
    }

    /**
     * Returns the station longitude.
     *
     * @return longitude
     */
    public double getLongitude() {
        return longitude;
    }

    /**
     * Returns the station capacity.
     *
     * @return capacity in kW
     */
    public double getCapacityKw() {
        return capacityKw;
    }

    /**
     * Returns the total battery slots.
     *
     * @return battery slot count
     */
    public int getBatterySlots() {
        return batterySlots;
    }

    /**
     * Returns whether the station is active.
     *
     * @return true when active
     */
    public boolean isActive() {
        return isActive;
    }
}