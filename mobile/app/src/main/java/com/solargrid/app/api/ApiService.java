/*
 * File:        ApiService.java
 * Author:      Shermon H (IT22177964)
 * Description: Retrofit interface for communicating with the central
 *              Solar Grid Web API.
 * Created:     30/09/2026
 */

package com.solargrid.app.api;

import com.solargrid.app.models.ApiMessage;
import com.solargrid.app.models.Booking;
import com.solargrid.app.models.CreateReservationRequest;
import com.solargrid.app.models.DashboardResponse;
import com.solargrid.app.models.LoginRequest;
import com.solargrid.app.models.LoginResponse;
import com.solargrid.app.models.Prosumer;
import com.solargrid.app.models.QrVerifyRequest;
import com.solargrid.app.models.RegisterRequest;
import com.solargrid.app.models.Reservation;
import com.solargrid.app.models.Slot;
import com.solargrid.app.models.Station;
import com.solargrid.app.models.UpdateProfileRequest;

import java.util.List;

import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.GET;
import retrofit2.http.PATCH;
import retrofit2.http.POST;
import retrofit2.http.PUT;
import retrofit2.http.Path;
import retrofit2.http.Query;

/**
 * Retrofit interface containing the REST API endpoints
 * used by the Android application.
 */
public interface ApiService {

    // ---------- Member 1: Authentication ----------

    /**
     * POST /api/auth/login - logs in with NIC and password.
     */
    @POST("auth/login")
    Call<LoginResponse> login(
            @Body LoginRequest request
    );


    // ---------- Member 2: Register, Profile, My Bookings, Dashboard ----------

    /**
     * POST /api/auth/register - registers a new prosumer.
     */
    @POST("auth/register")
    Call<ApiMessage> register(
            @Body RegisterRequest request
    );

    /**
     * GET /api/profile - gets the logged-in prosumer's profile.
     */
    @GET("profile")
    Call<Prosumer> getProfile();

    /**
     * PUT /api/profile - updates the logged-in prosumer's profile.
     */
    @PUT("profile")
    Call<ApiMessage> updateProfile(
            @Body UpdateProfileRequest request
    );

    /**
     * POST /api/profile/deactivate-request -
     * requests prosumer account deactivation.
     */
    @POST("profile/deactivate-request")
    Call<ApiMessage> requestDeactivation();

    /**
     * GET /api/dashboard/prosumer - gets prosumer dashboard counts.
     */
    @GET("dashboard/prosumer")
    Call<DashboardResponse> getDashboard();

    /**
     * GET /api/my/reservations - gets the logged-in prosumer's
     * bookings used by the existing bookings screen.
     *
     * @param type current or history
     * @param search optional search text
     * @return list of bookings
     */
    @GET("my/reservations")
    Call<List<Booking>> getBookings(
            @Query("type") String type,
            @Query("search") String search
    );

    /**
     * GET /api/my/reservations - gets the logged-in prosumer's
     * reservations for the M4 reservation screen.
     *
     * @param type current or history
     * @param search optional search text
     * @return list of reservations
     */
    @GET("my/reservations")
    Call<List<Reservation>> getMyReservations(
            @Query("type") String type,
            @Query("search") String search
    );


    // ---------- Member 3: Stations & Slots ----------

    /**
     * GET /api/stations - gets active solar stations.
     *
     * @param activeOnly whether only active stations should be returned
     * @return list of solar stations
     */
    @GET("stations")
    Call<List<Station>> getStations(
            @Query("activeOnly") boolean activeOnly
    );

    /**
     * GET /api/stations/{stationId}/slots - gets booking slots
     * for a selected station and date.
     *
     * @param stationId selected station ID
     * @param date reservation date in yyyy-MM-dd format
     * @return list of booking slots
     */
    @GET("stations/{stationId}/slots")
    Call<List<Slot>> getStationSlots(
            @Path("stationId") String stationId,
            @Query("date") String date
    );

    /**
     * POST /api/reservations/verify-qr - verifies a reservation QR code.
     *
     * @param request QR verification request
     * @return verification response
     */
    @POST("reservations/verify-qr")
    Call<Object> verifyQr(
            @Body QrVerifyRequest request
    );

    /**
     * PATCH /api/reservations/{id}/complete - completes a reservation.
     *
     * @param id reservation ID
     * @return completion response
     */
    @PATCH("reservations/{id}/complete")
    Call<Object> completeReservation(
            @Path("id") String id
    );


    // ---------- Member 4: Reservations ----------

    /**
     * GET /api/reservations/{id} - gets one reservation by ID.
     *
     * @param id reservation ID
     * @return reservation details
     */
    @GET("reservations/{id}")
    Call<Reservation> getReservation(
            @Path("id") String id
    );

    /**
     * POST /api/reservations - creates a new energy reservation.
     *
     * @param request reservation creation request
     * @return created reservation
     */
    @POST("reservations")
    Call<Reservation> createReservation(
            @Body CreateReservationRequest request
    );

    /**
     * PUT /api/reservations/{id} - updates an existing reservation.
     *
     * @param id reservation ID
     * @param request updated reservation details
     * @return updated reservation
     */
    @PUT("reservations/{id}")
    Call<Reservation> updateReservation(
            @Path("id") String id,
            @Body CreateReservationRequest request
    );

    /**
     * PATCH /api/reservations/{id}/cancel - cancels a reservation.
     *
     * @param id reservation ID
     * @return cancelled reservation
     */
    @PATCH("reservations/{id}/cancel")
    Call<Reservation> cancelReservation(
            @Path("id") String id
    );
}