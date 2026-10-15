/*
 * File:        DashboardResponse.java
 * Author:      Premaratne R.A.N.C (IT22050908)
 * Description: Dashboard counts returned by the API. Matches the
 *              DashboardResponse DTO on the server side.
 */
package com.solargrid.app.models;

/**
 * Dashboard counts returned by the API. Used by both the prosumer
 * dashboard and the Backoffice dashboard.
 */
public class DashboardResponse {

    /** Number of bookings (or prosumers, for Backoffice) with status Pending. */
    public long pendingCount;

    /** Number of approved bookings whose reservation time is in the future. */
    public long approvedFutureCount;
}