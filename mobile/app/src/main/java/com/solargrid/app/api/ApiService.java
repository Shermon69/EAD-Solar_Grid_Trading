/*
 * File:        ApiService.java
 * Author:      Shermon H (IT22177964)
 * Description: List of Web API endpoints the mobile app can call (Retrofit).
 *              Each member adds their own endpoints in their section below.
 * Created:     29/09/2026
 */

package com.solargrid.app.api;

import com.solargrid.app.models.LoginRequest;
import com.solargrid.app.models.LoginResponse;

import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.POST;

/**
 * Retrofit interface. Paths are relative to the base URL (http://.../api/).
 */
public interface ApiService {

    // ---------- Member 1: Auth ----------

    /**
     * POST /api/auth/login - logs in with NIC and password.
     */
    @POST("auth/login")
    Call<LoginResponse> login(@Body LoginRequest request);

    // ---------- Member 2: Register, profile, my bookings, dashboard ----------

    // ---------- Member 3: Stations, slots, QR verification ----------

    // ---------- Member 4: Reservations ----------
}
