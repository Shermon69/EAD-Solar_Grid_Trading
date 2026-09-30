/*
 * File:        ApiService.java
 * Author:      Shermon H (IT22177964)
 * Description: List of Web API endpoints the mobile app can call (Retrofit).
 *              Each member adds their own endpoints in their section below.
 * Created:     29/09/2026
 */

package com.solargrid.app.api;

import com.solargrid.app.models.ApiMessage;
import com.solargrid.app.models.Booking;
import com.solargrid.app.models.DashboardResponse;
import com.solargrid.app.models.LoginRequest;
import com.solargrid.app.models.LoginResponse;
import com.solargrid.app.models.Prosumer;
import com.solargrid.app.models.RegisterRequest;
import com.solargrid.app.models.UpdateProfileRequest;

import java.util.List;
import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.GET;
import retrofit2.http.POST;
import retrofit2.http.PUT;
import retrofit2.http.Query;

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

     /** POST /api/auth/register - registers a new prosumer (public). */
    @POST("auth/register")
    Call<ApiMessage> register(@Body RegisterRequest request);

    /** GET /api/profile - the logged-in prosumer's profile. */
    @GET("profile")
    Call<Prosumer> getProfile();

    /** PUT /api/profile - update the logged-in prosumer's profile. */
    @PUT("profile")
    Call<ApiMessage> updateProfile(@Body UpdateProfileRequest request);

    /** POST /api/profile/deactivate-request - ask Backoffice to deactivate. */
    @POST("profile/deactivate-request")
    Call<ApiMessage> requestDeactivation();

    /** GET /api/dashboard/prosumer - dashboard counts. */
    @GET("dashboard/prosumer")
    Call<DashboardResponse> getDashboard();

    /** GET /api/my/reservations - type = "current" or "history", search optional. */
    @GET("my/reservations")
    Call<List<Booking>> getBookings(@Query("type") String type, @Query("search") String search);

    // ---------- Member 3: Stations, slots, QR verification ----------

    // ---------- Member 4: Reservations ----------
}
