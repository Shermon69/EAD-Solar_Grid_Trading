/*
 * File:        QrVerifyRequest.java
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: Model for QR verification request.
 * Created:     29/09/2026
 */
package com.solargrid.app.models;

public class QrVerifyRequest {
    public String reservationId;
    public String qrToken;
    
    public QrVerifyRequest(String reservationId, String qrToken) {
        this.reservationId = reservationId;
        this.qrToken = qrToken;
    }
}
