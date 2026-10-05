/*
 * File:        CreateBookingActivity.java
 * Author:      Dissanayake D.M.S.N (IT22210692)
 * Description: Screen used by a prosumer to create an energy reservation.
 *              Station and slot data are loaded from the central Web API.
 */

package com.solargrid.app.activities;

import android.app.DatePickerDialog;
import android.os.Bundle;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Spinner;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import com.solargrid.app.R;
import com.solargrid.app.api.ApiClient;
import com.solargrid.app.api.ApiService;
import com.solargrid.app.db.SessionManager;
import com.solargrid.app.models.CreateReservationRequest;
import com.solargrid.app.models.Slot;
import com.solargrid.app.models.Station;
import com.solargrid.app.utils.DateUtils;

import java.util.ArrayList;
import java.util.Calendar;
import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

/**
 * Activity for creating an energy reservation.
 *
 * Station and slot information are retrieved from the central
 * Web API. The reservation is submitted through the same API.
 */
public class CreateBookingActivity extends AppCompatActivity {

    private Spinner spinnerStation;
    private Spinner spinnerSlot;
    private Spinner spinnerReservationType;
    private Button btnSelectDate;
    private Button btnCreateBooking;
    private EditText etEnergyKwh;

    private ApiService apiService;
    private SessionManager sessionManager;

    private String selectedDate = "";
    private String selectedStationId = "";
    private String selectedSlotId = "";

    private final List<Station> stationList = new ArrayList<>();
    private final List<Slot> slotList = new ArrayList<>();

    /**
     * Creates the booking screen and prepares its input controls.
     *
     * @param savedInstanceState previously saved activity state
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        setContentView(R.layout.activity_create_booking);

        initializeViews();

        apiService = ApiClient.getService(this);
        sessionManager = new SessionManager(this);

        setupReservationType();
        setupStationSelection();
        setupDatePicker();
        setupCreateButton();

        loadStations();
    }

    /**
     * Finds and stores references to the screen controls.
     */
    private void initializeViews() {

        spinnerStation = findViewById(R.id.spinnerStation);

        spinnerSlot = findViewById(R.id.spinnerSlot);

        spinnerReservationType =
                findViewById(R.id.spinnerReservationType);

        btnSelectDate =
                findViewById(R.id.btnSelectDate);

        btnCreateBooking =
                findViewById(R.id.btnCreateBooking);

        etEnergyKwh =
                findViewById(R.id.etEnergyKwh);
    }

    /**
     * Sets the available reservation types.
     */
    private void setupReservationType() {

        String[] reservationTypes = {
                "Energy Drop-off",
                "Charging"
        };

        ArrayAdapter<String> adapter =
                new ArrayAdapter<>(
                        this,
                        android.R.layout.simple_spinner_item,
                        reservationTypes
                );

        adapter.setDropDownViewResource(
                android.R.layout.simple_spinner_dropdown_item
        );

        spinnerReservationType.setAdapter(adapter);
    }

    /**
     * Loads active solar stations from the central Web API.
     */
    private void loadStations() {

        apiService.getStations(true)
                .enqueue(new Callback<List<Station>>() {

                    @Override
                    public void onResponse(
                            Call<List<Station>> call,
                            Response<List<Station>> response
                    ) {

                        if (!response.isSuccessful()
                                || response.body() == null) {

                            Toast.makeText(
                                    CreateBookingActivity.this,
                                    "Unable to load stations.",
                                    Toast.LENGTH_SHORT
                            ).show();

                            return;
                        }

                        stationList.clear();

                        stationList.addAll(response.body());

                        List<String> stationNames =
                                new ArrayList<>();

                        stationNames.add("Select Station");

                        for (Station station : stationList) {

                            stationNames.add(
                                    station.getName()
                            );
                        }

                        ArrayAdapter<String> adapter =
                                new ArrayAdapter<>(
                                        CreateBookingActivity.this,
                                        android.R.layout.simple_spinner_item,
                                        stationNames
                                );

                        adapter.setDropDownViewResource(
                                android.R.layout.simple_spinner_dropdown_item
                        );

                        spinnerStation.setAdapter(adapter);
                    }

                    @Override
                    public void onFailure(
                            Call<List<Station>> call,
                            Throwable t
                    ) {

                        Toast.makeText(
                                CreateBookingActivity.this,
                                "Unable to connect to server.",
                                Toast.LENGTH_SHORT
                        ).show();
                    }
                });
    }

    /**
     * Handles station selection and loads slots when a station
     * and reservation date are available.
     */
    private void setupStationSelection() {

        spinnerStation.setOnItemSelectedListener(
                new android.widget.AdapterView.OnItemSelectedListener() {

                    @Override
                    public void onItemSelected(
                            android.widget.AdapterView<?> parent,
                            android.view.View view,
                            int position,
                            long id
                    ) {

                        if (position == 0) {

                            selectedStationId = "";

                            clearSlots();

                            return;
                        }

                        int stationIndex = position - 1;

                        if (stationIndex >= 0
                                && stationIndex < stationList.size()) {

                            selectedStationId =
                                    stationList
                                            .get(stationIndex)
                                            .getId();

                            if (!selectedDate.isEmpty()) {

                                loadSlots();
                            }
                        }
                    }

                    @Override
                    public void onNothingSelected(
                            android.widget.AdapterView<?> parent
                    ) {

                        selectedStationId = "";
                    }
                }
        );
    }

    /**
     * Opens a date picker and stores the selected reservation date.
     */
    private void setupDatePicker() {

        btnSelectDate.setOnClickListener(v -> {

            Calendar calendar =
                    Calendar.getInstance();

            DatePickerDialog dialog =
                    new DatePickerDialog(
                            this,
                            (view, year, month, dayOfMonth) -> {

                                selectedDate =
                                        String.format(
                                                "%04d-%02d-%02d",
                                                year,
                                                month + 1,
                                                dayOfMonth
                                        );

                                btnSelectDate.setText(
                                        selectedDate
                                );

                                if (!selectedStationId.isEmpty()) {

                                    loadSlots();
                                }
                            },
                            calendar.get(Calendar.YEAR),
                            calendar.get(Calendar.MONTH),
                            calendar.get(Calendar.DAY_OF_MONTH)
                    );

            dialog.getDatePicker().setMinDate(
                    System.currentTimeMillis()
            );

            dialog.show();
        });
    }

    /**
     * Loads booking slots for the selected station and date.
     */
    private void loadSlots() {

        if (selectedStationId.isEmpty()
                || selectedDate.isEmpty()) {

            return;
        }

        apiService.getStationSlots(
                        selectedStationId,
                        selectedDate
                )
                .enqueue(new Callback<List<Slot>>() {

                    @Override
                    public void onResponse(
                            Call<List<Slot>> call,
                            Response<List<Slot>> response
                    ) {

                        if (!response.isSuccessful()
                                || response.body() == null) {

                            Toast.makeText(
                                    CreateBookingActivity.this,
                                    "Unable to load available slots.",
                                    Toast.LENGTH_SHORT
                            ).show();

                            clearSlots();

                            return;
                        }

                        slotList.clear();

                        for (Slot slot : response.body()) {

                            if (slot.isAvailable()
                                    && slot.getAvailableSlots() > 0) {

                                slotList.add(slot);
                            }
                        }

                        List<String> slotNames =
                                new ArrayList<>();

                        slotNames.add(
                                "Select Time Slot"
                        );

                        for (Slot slot : slotList) {

                            String startTime =
                                    formatTime(
                                            slot.getStartTime()
                                    );

                            String endTime =
                                    formatTime(
                                            slot.getEndTime()
                                    );

                            slotNames.add(
                                    startTime
                                            + " - "
                                            + endTime
                            );
                        }

                        ArrayAdapter<String> adapter =
                                new ArrayAdapter<>(
                                        CreateBookingActivity.this,
                                        android.R.layout.simple_spinner_item,
                                        slotNames
                                );

                        adapter.setDropDownViewResource(
                                android.R.layout.simple_spinner_dropdown_item
                        );

                        spinnerSlot.setAdapter(adapter);

                        spinnerSlot.setOnItemSelectedListener(
                                new android.widget.AdapterView.OnItemSelectedListener() {

                                    @Override
                                    public void onItemSelected(
                                            android.widget.AdapterView<?> parent,
                                            android.view.View view,
                                            int position,
                                            long id
                                    ) {

                                        if (position == 0) {

                                            selectedSlotId = "";

                                            return;
                                        }

                                        int slotIndex =
                                                position - 1;

                                        if (slotIndex >= 0
                                                && slotIndex < slotList.size()) {

                                            selectedSlotId =
                                                    slotList
                                                            .get(slotIndex)
                                                            .getId();
                                        }
                                    }

                                    @Override
                                    public void onNothingSelected(
                                            android.widget.AdapterView<?> parent
                                    ) {

                                        selectedSlotId = "";
                                    }
                                }
                        );

                        if (slotList.isEmpty()) {

                            Toast.makeText(
                                    CreateBookingActivity.this,
                                    "No available slots for this date.",
                                    Toast.LENGTH_SHORT
                            ).show();
                        }
                    }

                    @Override
                    public void onFailure(
                            Call<List<Slot>> call,
                            Throwable t
                    ) {

                        Toast.makeText(
                                CreateBookingActivity.this,
                                "Unable to connect to server.",
                                Toast.LENGTH_SHORT
                        ).show();

                        clearSlots();
                    }
                });
    }

    /**
     * Clears the slot spinner and currently selected slot.
     */
    private void clearSlots() {

        selectedSlotId = "";

        slotList.clear();

        List<String> emptySlots =
                new ArrayList<>();

        emptySlots.add("Select Time Slot");

        ArrayAdapter<String> adapter =
                new ArrayAdapter<>(
                        this,
                        android.R.layout.simple_spinner_item,
                        emptySlots
                );

        adapter.setDropDownViewResource(
                android.R.layout.simple_spinner_dropdown_item
        );

        spinnerSlot.setAdapter(adapter);
    }

    /**
     * Converts an API date-time value (UTC) into a readable time
     * in the phone's local time zone.
     *
     * Example (Sri Lanka):
     * 2026-10-03T02:30:00Z -> 08:00 AM
     *
     * @param time API date-time value in UTC
     * @return readable local time value
     */
    private String formatTime(String time) {

        return DateUtils.formatLocalTime(time);
    }

    /**
     * Handles the create reservation button.
     */
    private void setupCreateButton() {

        btnCreateBooking.setOnClickListener(v -> {

            String energyText =
                    etEnergyKwh.getText()
                            .toString()
                            .trim();

            if (spinnerStation.getSelectedItemPosition() == 0
                    || selectedStationId.isEmpty()) {

                Toast.makeText(
                        this,
                        "Please select a station.",
                        Toast.LENGTH_SHORT
                ).show();

                return;
            }

            if (selectedDate.isEmpty()) {

                Toast.makeText(
                        this,
                        "Please select a reservation date.",
                        Toast.LENGTH_SHORT
                ).show();

                return;
            }

            if (spinnerSlot.getSelectedItemPosition() == 0
                    || selectedSlotId.isEmpty()) {

                Toast.makeText(
                        this,
                        "Please select a time slot.",
                        Toast.LENGTH_SHORT
                ).show();

                return;
            }

            if (energyText.isEmpty()) {

                etEnergyKwh.setError(
                        "Enter energy amount"
                );

                etEnergyKwh.requestFocus();

                return;
            }

            double energyKwh;

            try {

                energyKwh =
                        Double.parseDouble(
                                energyText
                        );

            } catch (NumberFormatException e) {

                etEnergyKwh.setError(
                        "Enter a valid energy amount"
                );

                etEnergyKwh.requestFocus();

                return;
            }

            if (energyKwh <= 0) {

                etEnergyKwh.setError(
                        "Energy must be greater than 0"
                );

                etEnergyKwh.requestFocus();

                return;
            }

            String prosumerNic =
                    sessionManager.getNic();

            if (prosumerNic == null
                    || prosumerNic.isEmpty()) {

                Toast.makeText(
                        this,
                        "User session not found. Please login again.",
                        Toast.LENGTH_SHORT
                ).show();

                return;
            }

            String reservationType =
                    spinnerReservationType
                            .getSelectedItem()
                            .toString();

            /*
             * Find the selected slot and use its actual start time
             * as the reservation time.
             */
            Slot selectedSlot = null;

            for (Slot slot : slotList) {

                if (slot.getId() != null
                        && slot.getId().equals(selectedSlotId)) {

                    selectedSlot = slot;

                    break;
                }
            }

            if (selectedSlot == null
                    || selectedSlot.getStartTime() == null
                    || selectedSlot.getStartTime().isEmpty()) {

                Toast.makeText(
                        this,
                        "Unable to determine selected time slot.",
                        Toast.LENGTH_SHORT
                ).show();

                return;
            }

            String reservationTime =
                    selectedSlot.getStartTime();

            /*
             * If the API returns a date-time with a different date,
             * ensure the selected booking date remains the date
             * selected by the user.
             */
            String slotTime =
                    extractTimePart(
                            selectedSlot.getStartTime()
                    );

            if (!slotTime.isEmpty()) {

                reservationTime =
                        selectedDate
                                + "T"
                                + slotTime;
            }

            CreateReservationRequest request =
                    new CreateReservationRequest(
                            prosumerNic,
                            selectedStationId,
                            selectedSlotId,
                            reservationTime,
                            reservationType,
                            energyKwh
                    );

            submitReservation(request);
        });
    }

    /**
     * Extracts the time portion from an API date-time value.
     *
     * Example:
     * 2026-10-03T09:00:00 -> 09:00:00
     *
     * @param dateTime API date-time value
     * @return time portion of the value
     */
    private String extractTimePart(String dateTime) {

        if (dateTime == null
                || dateTime.isEmpty()) {

            return "";
        }

        int separatorIndex =
                dateTime.indexOf('T');

        if (separatorIndex >= 0
                && separatorIndex + 1 < dateTime.length()) {

            return dateTime.substring(
                    separatorIndex + 1
            );
        }

        separatorIndex =
                dateTime.indexOf(' ');

        if (separatorIndex >= 0
                && separatorIndex + 1 < dateTime.length()) {

            return dateTime.substring(
                    separatorIndex + 1
            );
        }

        return dateTime;
    }

    /**
     * Sends the reservation request to the central Web API.
     *
     * @param request reservation request data
     */
    private void submitReservation(
            CreateReservationRequest request
    ) {

        btnCreateBooking.setEnabled(false);

        apiService.createReservation(request)
                .enqueue(
                        new Callback<com.solargrid.app.models.Reservation>() {

                            @Override
                            public void onResponse(
                                    Call<com.solargrid.app.models.Reservation> call,
                                    Response<com.solargrid.app.models.Reservation> response
                            ) {

                                btnCreateBooking.setEnabled(true);

                                if (response.isSuccessful()
                                        && response.body() != null) {

                                    // Show the summary page for the new booking
                                    BookingSummaryActivity.open(
                                            CreateBookingActivity.this,
                                            BookingSummaryActivity.ACTION_CREATED,
                                            response.body()
                                    );

                                    finish();

                                } else {

                                    Toast.makeText(
                                            CreateBookingActivity.this,
                                            "Unable to create reservation.",
                                            Toast.LENGTH_LONG
                                    ).show();
                                }
                            }

                            @Override
                            public void onFailure(
                                    Call<com.solargrid.app.models.Reservation> call,
                                    Throwable t
                            ) {

                                btnCreateBooking.setEnabled(true);

                                Toast.makeText(
                                        CreateBookingActivity.this,
                                        "Unable to connect to server.",
                                        Toast.LENGTH_LONG
                                ).show();
                            }
                        }
                );
    }
}