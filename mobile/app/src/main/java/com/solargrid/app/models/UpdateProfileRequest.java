/*
 * File:        UpdateProfileRequest.java
 * Author:      Premaratne R.A.N.C (IT22050908)
 * Description: Request body for editing the prosumer profile. Matches the
 *              UpdateProfileRequest DTO on the server side.
 * Created:     29/09/2026
 */
package com.solargrid.app.models;

/**
 * Request sent to the API when a prosumer edits their profile. All fields
 * except newPassword are required; newPassword may be null to keep the
 * existing password.
 */
public class UpdateProfileRequest {

    /** Full name of the prosumer (3 to 100 characters). */
    public String fullName;

    /** Contact email address. */
    public String email;

    /** Sri Lankan mobile number (10 digits, starts with 0). */
    public String phone;

    /** Home or business address (up to 200 characters). */
    public String address;

    /** Optional new password (minimum 8 characters). Null keeps the old one. */
    public String newPassword;

    /**
     * Creates an update request. Pass {@code null} for newPassword to keep
     * the existing password.
     *
     * @param fullName     full name of the prosumer
     * @param email        contact email address
     * @param phone        Sri Lankan mobile number (10 digits)
     * @param address      home or business address
     * @param newPassword  optional new password (null = keep old password)
     */
    public UpdateProfileRequest(String fullName, String email, String phone,
                                String address, String newPassword) {
        this.fullName = fullName;
        this.email = email;
        this.phone = phone;
        this.address = address;
        this.newPassword = newPassword;
    }
}