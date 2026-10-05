/*
 * File:        LoginRequest.java
 * Author:      Shermon H (IT22177964)
 * Description: Data sent to POST /api/auth/login.
 */

package com.solargrid.app.models;

/**
 * NIC and password typed on the login screen.
 */
public class LoginRequest {

    private final String nic;
    private final String password;

    /**
     * Creates a login request with the given NIC and password.
     */
    public LoginRequest(String nic, String password) {
        this.nic = nic;
        this.password = password;
    }

    /**
     * Returns the NIC.
     */
    public String getNic() {
        return nic;
    }

    /**
     * Returns the password.
     */
    public String getPassword() {
        return password;
    }
}
