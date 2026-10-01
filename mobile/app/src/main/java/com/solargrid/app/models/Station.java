/*
 * File:        Station.java
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: Model for Solar Station.
 * Created:     29/09/2026
 */
package com.solargrid.app.models;

import java.io.Serializable;

public class Station implements Serializable {
    public String id;
    public String name;
    public String address;
    public double latitude;
    public double longitude;
    public double capacityKw;
    public int batterySlots;
    public boolean isActive;
}
