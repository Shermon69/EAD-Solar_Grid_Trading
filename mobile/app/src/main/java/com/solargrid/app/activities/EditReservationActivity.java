/*
 * File:        EditReservationActivity.java
 * Author:      Dissanayake D.M.S.N (IT22210692)
 * Description: Screen used by a prosumer to edit an existing energy
 *              reservation through the central Web API.
 * Created:     30/09/2026
 */

package com.solargrid.app.activities;

import android.app.DatePickerDialog;
import android.os.Bundle;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ProgressBar;
import android.widget.Spinner;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import com.solargrid.app.R;
import com.solargrid.app.api.ApiClient;
import com.solargrid.app.api.ApiService;
import com.solargrid.app.models.CreateReservationRequest;
import com.solargrid.app.models.Reservation;
import com.solargrid.app.models.Slot;
import com.solargrid.app.models.Station;

import java.util.ArrayList;
import java.util.Calendar;
import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

/**
 * Activity used to edit an existing energy reservation.
 */
public class EditReservationActivity extends AppCompatActivity {

    private Spinner spinnerEditStation;
    private Spinner spinnerEditSlot;
    private Spinner spinnerEditReservationType;

    private Button btnEditDate;
    private Button btnSaveReservation;

    private EditText etEditEnergyKwh;

    private ProgressBar progressEditReservation;

    private ApiService apiService;

    private String reservationId = "";
    private String prosumerNic = "";

    private String selectedDate = "";
    private String selectedStationId = "";
    private String selectedSlotId = "";

    private final List<Station> stationList = new ArrayList<>();
    private final List<Slot> slotList = new ArrayList<>();

    /**
     * Creates the edit reservation screen.
     *
     * @param savedInstanceState previously saved activity state
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        setContentView(R.layout.activity_edit_reservation);

        initializeViews();

        apiService = ApiClient.getService(this);

        reservationId =
                getIntent().getStringExtra("reservationId");

        if (reservationId == null || reservationId.isEmpty()) {

            Toast.makeText(
                    this,
                    "Reservation ID was not provided.",
                    Toast.LENGTH_LONG
            ).show();

            finish();

            return;
        }

        setupReservationTypes();
        setupDatePicker();
        setupStationSelection();
        setupSaveButton();

        loadReservation();
    }

    /**
     * Finds and stores references to the screen controls.
     */
    private void initializeViews() {

        spinnerEditStation =
                findViewById(R.id.spinnerEditStation);

        spinnerEditSlot =
                findViewById(R.id.spinnerEditSlot);

        spinnerEditReservationType =
                findViewById(R.id.spinnerEditReservationType);

        btnEditDate =
                findViewById(R.id.btnEditDate);

        btnSaveReservation =
                findViewById(R.id.btnSaveReservation);

        etEditEnergyKwh =
                findViewById(R.id.etEditEnergyKwh);

        progressEditReservation =
                findViewById(R.id.progressEditReservation);
    }

    /**
     * Sets the available reservation types.
     */
    private void setupReservationTypes() {

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

        spinnerEditReservationType.setAdapter(adapter);
    }

    /**
     * Loads the existing reservation from the central Web API.
     */
    private void loadReservation() {

        setSaving(true);

        apiService.getReservation(
                reservationId
        ).enqueue(new Callback<Reservation>() {

            @Override
            public void onResponse(
                    Call<Reservation> call,
                    Response<Reservation> response
            ) {

                setSaving(false);

                if (!response.isSuccessful()
                        || response.body() == null) {

                    Toast.makeText(
                            EditReservationActivity.this,
                            "Unable to load reservation.",
                            Toast.LENGTH_LONG
                    ).show();

                    return;
                }

                Reservation reservation = response.body();

                prosumerNic =
                        reservation.getProsumerNic();

                selectedDate =
                        extractDate(
                                reservation.getReservationTime()
                        );

                selectedStationId =
                        reservation.getStationId();

                selectedSlotId =
                        reservation.getSlotId();

                etEditEnergyKwh.setText(
                        String.valueOf(
                                reservation.getEnergyKwh()
                        )
                );

                selectReservationType(
                        reservation.getType()
                );

                btnEditDate.setText(selectedDate);

                /*
                 * Station and slot lists are loaded through the
                 * central API. They require the Member 3 endpoints.
                 */
                loadStations();
            }

            @Override
            public void onFailure(
                    Call<Reservation> call,
                    Throwable t
            ) {

                setSaving(false);

                Toast.makeText(
                        EditReservationActivity.this,
                        "Unable to connect to server.",
                        Toast.LENGTH_LONG
                ).show();
            }
        });
    }

    /**
     * Loads active stations from the central Web API.
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

                            return;
                        }

                        stationList.clear();

                        stationList.addAll(
                                response.body()
                        );

                        List<String> stationNames =
                                new ArrayList<>();

                        stationNames.add("Select Station");

                        int selectedPosition = 0;

                        for (int i = 0;
                             i < stationList.size();
                             i++) {

                            Station station =
                                    stationList.get(i);

                            stationNames.add(
                                    station.getName()
                            );

                            if (station.getId() != null
                                    && station.getId().equals(
                                    selectedStationId)) {

                                selectedPosition = i + 1;
                            }
                        }

                        ArrayAdapter<String> adapter =
                                new ArrayAdapter<>(
                                        EditReservationActivity.this,
                                        android.R.layout.simple_spinner_item,
                                        stationNames
                                );

                        adapter.setDropDownViewResource(
                                android.R.layout.simple_spinner_dropdown_item
                        );

                        spinnerEditStation.setAdapter(adapter);

                        if (selectedPosition > 0) {

                            spinnerEditStation.setSelection(
                                    selectedPosition
                            );

                            if (!selectedDate.isEmpty()) {
                                loadSlots();
                            }
                        }
                    }

                    @Override
                    public void onFailure(
                            Call<List<Station>> call,
                            Throwable t
                    ) {
                        // Station API will be available after
                        // Member 3 integration.
                    }
                });
    }

    /**
     * Configures station selection.
     */
    private void setupStationSelection() {

        spinnerEditStation.setOnItemSelectedListener(
                new android.widget.AdapterView.OnItemSelectedListener() {

                    @Override
                    public void onItemSelected(
                            android.widget.AdapterView<?> parent,
                            View view,
                            int position,
                            long id
                    ) {

                        if (position == 0) {

                            selectedStationId = "";

                            clearSlots();

                            return;
                        }

                        int stationIndex =
                                position - 1;

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
     * Loads available slots for the selected station and date.
     */
    private void loadSlots() {

        if (selectedStationId.isEmpty()
                || selectedDate.isEmpty()) {

            return;
        }

        apiService.getStationSlots(
                selectedStationId,
                selectedDate
        ).enqueue(new Callback<List<Slot>>() {

            @Override
            public void onResponse(
                    Call<List<Slot>> call,
                    Response<List<Slot>> response
            ) {

                if (!response.isSuccessful()
                        || response.body() == null) {

                    clearSlots();

                    return;
                }

                slotList.clear();

                slotList.addAll(
                        response.body()
                );

                List<String> slotNames =
                        new ArrayList<>();

                slotNames.add("Select Time Slot");

                int selectedPosition = 0;

                for (int i = 0;
                     i < slotList.size();
                     i++) {

                    Slot slot =
                            slotList.get(i);

                    if (!slot.isAvailable()
                            && (slot.getId() == null
                            || !slot.getId().equals(selectedSlotId))) {

                        continue;
                    }

                    String start =
                            formatTime(
                                    slot.getStartTime()
                            );

                    String end =
                            formatTime(
                                    slot.getEndTime()
                            );

                    slotNames.add(
                            start + " - " + end
                    );

                    if (slot.getId() != null
                            && slot.getId().equals(
                            selectedSlotId)) {

                        selectedPosition =
                                slotNames.size() - 1;
                    }
                }

                ArrayAdapter<String> adapter =
                        new ArrayAdapter<>(
                                EditReservationActivity.this,
                                android.R.layout.simple_spinner_item,
                                slotNames
                        );

                adapter.setDropDownViewResource(
                        android.R.layout.simple_spinner_dropdown_item
                );

                spinnerEditSlot.setAdapter(adapter);

                spinnerEditSlot.setOnItemSelectedListener(
                        new android.widget.AdapterView.OnItemSelectedListener() {

                            @Override
                            public void onItemSelected(
                                    android.widget.AdapterView<?> parent,
                                    View view,
                                    int position,
                                    long id
                            ) {

                                if (position == 0) {

                                    selectedSlotId = "";

                                    return;
                                }

                                int actualIndex =
                                        getAvailableSlotIndex(
                                                position - 1
                                        );

                                if (actualIndex >= 0
                                        && actualIndex < slotList.size()) {

                                    selectedSlotId =
                                            slotList
                                                    .get(actualIndex)
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

                if (selectedPosition > 0) {

                    spinnerEditSlot.setSelection(
                            selectedPosition
                    );
                }
            }

            @Override
            public void onFailure(
                    Call<List<Slot>> call,
                    Throwable t
            ) {
                clearSlots();
            }
        });
    }

    /**
     * Gets the actual slot index while ignoring unavailable slots.
     *
     * @param visiblePosition position shown in the spinner
     * @return index in the slot list
     */
    private int getAvailableSlotIndex(
            int visiblePosition
    ) {

        int currentVisiblePosition = 0;

        for (int i = 0;
             i < slotList.size();
             i++) {

            Slot slot = slotList.get(i);

            if (!slot.isAvailable()
                    && (slot.getId() == null
                    || !slot.getId().equals(selectedSlotId))) {

                continue;
            }

            if (currentVisiblePosition == visiblePosition) {
                return i;
            }

            currentVisiblePosition++;
        }

        return -1;
    }

    /**
     * Clears the slot spinner.
     */
    private void clearSlots() {

        selectedSlotId = "";

        slotList.clear();

        List<String> slots =
                new ArrayList<>();

        slots.add("Select Time Slot");

        ArrayAdapter<String> adapter =
                new ArrayAdapter<>(
                        this,
                        android.R.layout.simple_spinner_item,
                        slots
                );

        adapter.setDropDownViewResource(
                android.R.layout.simple_spinner_dropdown_item
        );

        spinnerEditSlot.setAdapter(adapter);
    }

    /**
     * Configures the date picker.
     */
    private void setupDatePicker() {

        btnEditDate.setOnClickListener(v -> {

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

                                btnEditDate.setText(
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
     * Configures the save button.
     */
    private void setupSaveButton() {

        btnSaveReservation.setOnClickListener(
                v -> updateReservation()
        );
    }

    /**
     * Updates the existing reservation through the API.
     */
    private void updateReservation() {

        String energyText =
                etEditEnergyKwh.getText()
                        .toString()
                        .trim();

        if (selectedStationId.isEmpty()) {

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

        if (selectedSlotId.isEmpty()) {

            Toast.makeText(
                    this,
                    "Please select a time slot.",
                    Toast.LENGTH_SHORT
            ).show();

            return;
        }

        if (energyText.isEmpty()) {

            etEditEnergyKwh.setError(
                    "Enter energy amount"
            );

            etEditEnergyKwh.requestFocus();

            return;
        }

        double energyKwh;

        try {

            energyKwh =
                    Double.parseDouble(
                            energyText
                    );

        } catch (NumberFormatException e) {

            etEditEnergyKwh.setError(
                    "Enter a valid energy amount"
            );

            etEditEnergyKwh.requestFocus();

            return;
        }

        if (energyKwh <= 0) {

            etEditEnergyKwh.setError(
                    "Energy must be greater than 0"
            );

            etEditEnergyKwh.requestFocus();

            return;
        }

        String reservationType =
                spinnerEditReservationType
                        .getSelectedItem()
                        .toString();

        String reservationTime =
                selectedDate + "T00:00:00";

        CreateReservationRequest request =
                new CreateReservationRequest(
                        prosumerNic,
                        selectedStationId,
                        selectedSlotId,
                        reservationTime,
                        reservationType,
                        energyKwh
                );

        sendUpdateRequest(request);
    }

    /**
     * Sends the update request to the central Web API.
     *
     * @param request updated reservation data
     */
    private void sendUpdateRequest(
            CreateReservationRequest request
    ) {

        setSaving(true);

        apiService.updateReservation(
                reservationId,
                request
        ).enqueue(
                new Callback<Reservation>() {

                    @Override
                    public void onResponse(
                            Call<Reservation> call,
                            Response<Reservation> response
                    ) {

                        setSaving(false);

                        if (response.isSuccessful()
                                && response.body() != null) {

                            Toast.makeText(
                                    EditReservationActivity.this,
                                    "Reservation updated successfully.",
                                    Toast.LENGTH_LONG
                            ).show();

                            finish();

                        } else {

                            Toast.makeText(
                                    EditReservationActivity.this,
                                    "Unable to update reservation.",
                                    Toast.LENGTH_LONG
                            ).show();
                        }
                    }

                    @Override
                    public void onFailure(
                            Call<Reservation> call,
                            Throwable t
                    ) {

                        setSaving(false);

                        Toast.makeText(
                                EditReservationActivity.this,
                                "Unable to connect to server.",
                                Toast.LENGTH_LONG
                        ).show();
                    }
                }
        );
    }

    /**
     * Selects the reservation type in the spinner.
     *
     * @param type reservation type
     */
    private void selectReservationType(
            String type
    ) {

        if (type == null) {
            return;
        }

        ArrayAdapter<?> adapter =
                (ArrayAdapter<?>)
                        spinnerEditReservationType
                                .getAdapter();

        if (adapter == null) {
            return;
        }

        for (int i = 0;
             i < adapter.getCount();
             i++) {

            Object item =
                    adapter.getItem(i);

            if (item != null
                    && item.toString()
                    .equalsIgnoreCase(type)) {

                spinnerEditReservationType
                        .setSelection(i);

                return;
            }
        }
    }

    /**
     * Extracts the date portion from an API date/time value.
     *
     * @param reservationTime reservation date/time
     * @return date in yyyy-MM-dd format
     */
    private String extractDate(
            String reservationTime
    ) {

        if (reservationTime == null
                || reservationTime.isEmpty()) {

            return "";
        }

        if (reservationTime.contains("T")) {

            return reservationTime.substring(
                    0,
                    Math.min(
                            10,
                            reservationTime.length()
                    )
            );
        }

        return reservationTime;
    }

    /**
     * Formats an API time value for display.
     *
     * @param time API time value
     * @return readable time
     */
    private String formatTime(String time) {

        if (time == null || time.isEmpty()) {
            return "";
        }

        if (time.length() >= 5) {
            return time.substring(0, 5);
        }

        return time;
    }

    /**
     * Enables or disables the save button while processing.
     *
     * @param saving whether a request is currently being processed
     */
    private void setSaving(boolean saving) {

        btnSaveReservation.setEnabled(!saving);

        progressEditReservation.setVisibility(
                saving
                        ? View.VISIBLE
                        : View.GONE
        );
    }
}