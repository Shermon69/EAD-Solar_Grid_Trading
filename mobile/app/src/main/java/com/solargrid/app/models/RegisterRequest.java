/*
 * File:        RegisterRequest.java
 * Author:      Premaratne R.A.N.C (IT22050908)
 * Description: Request body for prosumer registration. Matches the
 *              RegisterRequest DTO on the server side.
 */
package com.solargrid.app.models;

/**
 * Registration request sent to the API when a new prosumer signs up.
 * The server validates all fields, hashes the password, and creates the
 * account with Pending status.
 */
public class RegisterRequest {

    /** NIC in the format 200012345678 or 991234567V. Primary key. */
    public String nic;

    /** Full name of the prosumer (3 to 100 characters). */
    public String fullName;

    /** Contact email address. */
    public String email;

    /** Sri Lankan mobile number (10 digits, starts with 0). */
    public String phone;

    /** Home or business address (up to 200 characters). */
    public String address;

    /** Plain password (minimum 8 characters). Hashed by the server. */
    public String password;

    /**
     * Creates a registration request with all required fields.
     *
     * @param nic       NIC in the format 200012345678 or 991234567V
     * @param fullName  full name of the prosumer
     * @param email     contact email address
     * @param phone     Sri Lankan mobile number (10 digits)
     * @param address   home or business address
     * @param password  plain password (minimum 8 characters)
     */
    public RegisterRequest(String nic, String fullName, String email,
                           String phone, String address, String password) {
        this.nic = nic;
        this.fullName = fullName;
        this.email = email;
        this.phone = phone;
        this.address = address;
        this.password = password;
    }
}