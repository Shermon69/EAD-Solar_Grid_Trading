/*
 * File:        Slot.java
 * Author:      Dissanayake D.M.S.N (IT22210692)
 * Description: Model representing an energy booking slot returned
 *              by the Web API.
 * Created:     30/09/2026
 */

package com.solargrid.app.models;

import com.google.gson.annotations.SerializedName;

/**
 * Represents an available energy booking slot.
 */
public class Slot {

    private String id;
    private String stationId;

    @SerializedName("startTime")
    private String startTime;

    @SerializedName("endTime")
    private String endTime;

    private int totalSlots;
    private int availableSlots;
    private boolean isAvailable;

    public Slot() {
    }

    /**
     * Returns the slot ID.
     *
     * @return slot ID
     */
    public String getId() {
        return id;
    }

    /**
     * Returns the station ID associated with this slot.
     *
     * @return station ID
     */
    public String getStationId() {
        return stationId;
    }

    /**
     * Returns the slot start time.
     *
     * @return start time
     */
    public String getStartTime() {
        return startTime;
    }

    /**
     * Returns the slot end time.
     *
     * @return end time
     */
    public String getEndTime() {
        return endTime;
    }

    /**
     * Returns the total number of slots.
     *
     * @return total slots
     */
    public int getTotalSlots() {
        return totalSlots;
    }

    /**
     * Returns the number of currently available slots.
     *
     * @return available slots
     */
    public int getAvailableSlots() {
        return availableSlots;
    }

    /**
     * Returns whether this slot is currently available.
     *
     * @return true when the slot is available
     */
    public boolean isAvailable() {
        return isAvailable;
    }
}