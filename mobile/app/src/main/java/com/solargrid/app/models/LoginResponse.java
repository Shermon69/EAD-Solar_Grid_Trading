/*
 * File:        LoginResponse.java
 * Author:      Shermon H (IT22177964)
 * Description: Data returned by POST /api/auth/login: the JWT token and the
 *              user's details. Field names match the API's JSON.
 * Created:     29/09/2026
 */

package com.solargrid.app.models;

/**
 * Token and user details returned after a successful login.
 */
public class LoginResponse {

    private String token;
    private String nic;
    private String fullName;
    private String role;

    /**
     * Returns the JWT token that must be sent with every later request.
     */
    public String getToken() {
        return token;
    }

    /**
     * Returns the user's NIC.
     */
    public String getNic() {
        return nic;
    }

    /**
     * Returns the user's full name.
     */
    public String getFullName() {
        return fullName;
    }

    /**
     * Returns the user's role: Prosumer, GridOperator or Backoffice.
     */
    public String getRole() {
        return role;
    }
}
