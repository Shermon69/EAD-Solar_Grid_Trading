/*
 * File:        Prosumer.java
 * Author:      Premaratne R.A.N.C (IT22050908)
 * Description: Profile returned by the API. Matches the ProsumerResponse
 *              DTO on the server side.
 */
package com.solargrid.app.models;

/**
 * A prosumer profile returned by the API. Never contains the password hash.
 * Used by the profile screen and the operator verification flow.
 */
public class Prosumer {

    /** NIC in the format 200012345678 or 991234567V. Also the primary key. */
    public String nic;

    /** Full name of the prosumer. */
    public String fullName;

    /** Contact email address. */
    public String email;

    /** Contact phone number (10 digits, starts with 0). */
    public String phone;

    /** Home or business address. */
    public String address;

    /** One of: Pending, Active, Deactivated. */
    public String status;

    /** True if the prosumer asked Backoffice to deactivate the account. */
    public boolean deactivationRequested;
}