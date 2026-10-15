/*
 * File:        StationDetailsActivity.java
 * Author:      WMVSB Wahundeniya (IT22292872)
 * Description: Details of one solar station, opened from the map. Shows its
 *              status, capacity, battery slots, a small map with directions,
 *              weekly opening hours (GET /api/stations/{id}) and the live slot
 *              availability for the next 7 days (GET /api/stations/{id}/slots).
 * Created:     29/09/2026
 */
package com.solargrid.app.activities;

import android.content.Intent;
import android.content.res.ColorStateList;
import android.graphics.Typeface;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.ImageButton;
import android.widget.LinearLayout;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.content.ContextCompat;

import com.google.android.material.chip.Chip;
import com.google.android.material.chip.ChipGroup;
import com.google.android.material.progressindicator.LinearProgressIndicator;
import com.solargrid.app.R;
import com.solargrid.app.api.ApiClient;
import com.solargrid.app.db.SessionManager;
import com.solargrid.app.maps.StationMaps;
import com.solargrid.app.models.OperatingHours;
import com.solargrid.app.models.Slot;
import com.solargrid.app.models.Station;
import com.solargrid.app.utils.Constants;
import com.solargrid.app.utils.DateUtils;

import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.Date;
import java.util.List;
import java.util.Locale;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

/**
 * Shows the details of a single solar station.
 */
public class StationDetailsActivity extends AppCompatActivity {

    // Intent extras used by MapActivity to open this screen
    public static final String EXTRA_STATION = "station";
    public static final String EXTRA_DISTANCE_METRES = "distance_metres";

    // Prosumers can book up to 7 days ahead, so availability is shown for the same range
    private static final int DAYS_TO_SHOW = 7;

    private Station station;
    private String selectedDate;

    private TextView tvHeaderTitle;
    private TextView tvHeaderSubtitle;
    private TextView tvStatus;
    private TextView tvDistance;
    private TextView tvOpenNow;
    private TextView tvCapacity;
    private TextView tvBatterySlots;
    private TextView tvAddress;
    private TextView tvCoordinates;
    private LinearLayout layoutSchedule;
    private TextView tvNoSchedule;
    private LinearProgressIndicator progressSlots;
    private LinearLayout layoutSlots;
    private TextView tvSlotsMessage;
    private View btnBookHere;

    /**
     * Reads the station passed from the map, shows it straight away, then
     * refreshes it and its slots from the Web API.
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_station_details);

        station = (Station) getIntent().getSerializableExtra(EXTRA_STATION);
        if (station == null) {
            finish();
            return;
        }

        tvHeaderTitle = findViewById(R.id.tvHeaderTitle);
        tvHeaderSubtitle = findViewById(R.id.tvHeaderSubtitle);
        tvStatus = findViewById(R.id.tvStatus);
        tvDistance = findViewById(R.id.tvDistance);
        tvOpenNow = findViewById(R.id.tvOpenNow);
        tvCapacity = findViewById(R.id.tvCapacity);
        tvBatterySlots = findViewById(R.id.tvBatterySlots);
        tvAddress = findViewById(R.id.tvAddress);
        tvCoordinates = findViewById(R.id.tvCoordinates);
        layoutSchedule = findViewById(R.id.layoutSchedule);
        tvNoSchedule = findViewById(R.id.tvNoSchedule);
        progressSlots = findViewById(R.id.progressSlots);
        layoutSlots = findViewById(R.id.layoutSlots);
        tvSlotsMessage = findViewById(R.id.tvSlotsMessage);
        btnBookHere = findViewById(R.id.btnBookHere);

        findViewById(R.id.btnHeaderBack).setOnClickListener(v -> finish());
        ImageButton btnRefresh = findViewById(R.id.btnHeaderAction);
        btnRefresh.setImageResource(R.drawable.ic_refresh);
        btnRefresh.setContentDescription(getString(R.string.btn_refresh));
        btnRefresh.setVisibility(View.VISIBLE);
        btnRefresh.setOnClickListener(v -> {
            loadStation();
            loadSlots();
        });

        findViewById(R.id.btnDirections).setOnClickListener(v -> MapActivity.openDirections(this, station));
        btnBookHere.setOnClickListener(v -> openCreateBooking());

        double distance = getIntent().getDoubleExtra(EXTRA_DISTANCE_METRES, -1);
        if (distance >= 0) {
            tvDistance.setText(MapActivity.formatDistance(distance) + " away");
            tvDistance.setVisibility(View.VISIBLE);
        }

        bindStation();
        showMapPreview();
        setupDayChips();
        loadStation();
    }

    /**
     * Fills every view with the current station data.
     */
    private void bindStation() {
        tvHeaderTitle.setText(station.name);
        tvHeaderSubtitle.setText(getString(R.string.station_details_title));
        tvCapacity.setText(String.format(Locale.ENGLISH, "%.0f kW/h", station.capacityKw));
        tvBatterySlots.setText(String.valueOf(station.batterySlots));
        tvAddress.setText(station.address);
        tvCoordinates.setText(String.format(Locale.ENGLISH, "%.5f, %.5f", station.latitude, station.longitude));

        tvStatus.setText(station.isActive ? R.string.station_active : R.string.station_inactive);
        int statusColor = station.isActive ? R.color.success : R.color.danger;
        int statusBackground = station.isActive ? R.color.success_light : R.color.danger_light;
        tvStatus.setTextColor(ContextCompat.getColor(this, statusColor));
        tvStatus.setBackgroundTintList(ColorStateList.valueOf(ContextCompat.getColor(this, statusBackground)));

        // Only prosumers book, and only at stations that are still active
        boolean isProsumer = Constants.ROLE_PROSUMER.equals(new SessionManager(this).getRole());
        btnBookHere.setVisibility(isProsumer && station.isActive ? View.VISIBLE : View.GONE);

        bindSchedule();
    }

    /**
     * Lists the weekly opening hours with today highlighted, and shows an
     * "Open now" / "Closed now" pill.
     */
    private void bindSchedule() {
        layoutSchedule.removeAllViews();
        List<OperatingHours> schedule = station.getSchedule();
        if (schedule == null || schedule.isEmpty()) {
            tvNoSchedule.setVisibility(View.VISIBLE);
            tvOpenNow.setVisibility(View.GONE);
            return;
        }
        tvNoSchedule.setVisibility(View.GONE);

        String today = new SimpleDateFormat("EEEE", Locale.ENGLISH).format(new Date());
        OperatingHours todayHours = null;

        for (OperatingHours hours : schedule) {
            View row = LayoutInflater.from(this).inflate(R.layout.item_detail_row, layoutSchedule, false);
            TextView label = row.findViewById(R.id.tvRowLabel);
            TextView value = row.findViewById(R.id.tvRowValue);
            label.setText(hours.getDay());
            value.setText(hours.getOpenTime() + " – " + hours.getCloseTime());

            if (today.equalsIgnoreCase(hours.getDay())) {
                todayHours = hours;
                label.setText(hours.getDay() + " (today)");
                label.setTextColor(ContextCompat.getColor(this, R.color.navy));
                label.setTypeface(label.getTypeface(), Typeface.BOLD);
                value.setTextColor(ContextCompat.getColor(this, R.color.solar));
            }
            layoutSchedule.addView(row);
        }

        if (!station.isActive) {
            tvOpenNow.setVisibility(View.GONE);
            return;
        }

        // Times are "HH:mm", so comparing the text gives the right order
        String now = new SimpleDateFormat("HH:mm", Locale.ENGLISH).format(new Date());
        boolean open = todayHours != null
                && now.compareTo(todayHours.getOpenTime()) >= 0
                && now.compareTo(todayHours.getCloseTime()) < 0;
        tvOpenNow.setText(open ? "Open now · until " + todayHours.getCloseTime() : "Closed now");
        int color = open ? R.color.success : R.color.danger;
        tvOpenNow.setTextColor(ContextCompat.getColor(this, color));
        tvOpenNow.setCompoundDrawableTintList(ColorStateList.valueOf(ContextCompat.getColor(this, color)));
        tvOpenNow.setBackgroundTintList(ColorStateList.valueOf(ContextCompat.getColor(this,
                open ? R.color.success_light : R.color.danger_light)));
        tvOpenNow.setVisibility(View.VISIBLE);
    }

    /**
     * Adds a small map of the station (Google Maps or OpenStreetMap, same as
     * the main map screen). Tapping it opens directions.
     */
    private void showMapPreview() {
        StationMaps.showPreview(this, findViewById(R.id.mapPreviewContainer), station,
                () -> MapActivity.openDirections(this, station));
    }

    /**
     * Creates one chip per day (Today, Tomorrow, Sat 18 ...) for the next
     * 7 days. Choosing a chip loads that day's slots.
     */
    private void setupDayChips() {
        ChipGroup group = findViewById(R.id.chipGroupDays);
        SimpleDateFormat apiFormat = new SimpleDateFormat("yyyy-MM-dd", Locale.ENGLISH);
        SimpleDateFormat labelFormat = new SimpleDateFormat("EEE d", Locale.ENGLISH);
        Calendar day = Calendar.getInstance();

        for (int i = 0; i < DAYS_TO_SHOW; i++) {
            Chip chip = new Chip(this);
            chip.setId(View.generateViewId());
            chip.setTag(apiFormat.format(day.getTime()));
            chip.setText(i == 0 ? "Today" : i == 1 ? "Tomorrow" : labelFormat.format(day.getTime()));
            chip.setCheckable(true);
            chip.setCheckedIconVisible(false);
            chip.setChipBackgroundColor(ContextCompat.getColorStateList(this, R.color.chip_radius_background));
            chip.setTextColor(ContextCompat.getColorStateList(this, R.color.chip_radius_text));
            chip.setChipStrokeColor(ColorStateList.valueOf(ContextCompat.getColor(this, R.color.divider)));
            chip.setChipStrokeWidth(getResources().getDisplayMetrics().density);
            group.addView(chip);
            day.add(Calendar.DAY_OF_MONTH, 1);
        }

        group.setOnCheckedStateChangeListener((chipGroup, checkedIds) -> {
            if (checkedIds.isEmpty()) {
                return;
            }
            Chip checked = chipGroup.findViewById(checkedIds.get(0));
            selectedDate = (String) checked.getTag();
            loadSlots();
        });

        // Selecting "Today" triggers the first slot load
        ((Chip) group.getChildAt(0)).setChecked(true);
    }

    /**
     * Gets the latest station data (status and opening hours) from the API.
     * If the phone is offline the station from the map is kept.
     */
    private void loadStation() {
        if (station.id == null) {
            return;
        }
        ApiClient.getService(this).getStation(station.id).enqueue(new Callback<Station>() {
            @Override
            public void onResponse(@NonNull Call<Station> call, @NonNull Response<Station> response) {
                if (!isFinishing() && response.isSuccessful() && response.body() != null) {
                    station = response.body();
                    bindStation();
                }
            }

            @Override
            public void onFailure(@NonNull Call<Station> call, @NonNull Throwable t) {
                // Offline: keep showing the cached station; the schedule card explains why it is empty
            }
        });
    }

    /**
     * Loads the time blocks for the selected day from GET /api/stations/{id}/slots.
     */
    private void loadSlots() {
        if (station.id == null || selectedDate == null) {
            return;
        }
        progressSlots.setVisibility(View.VISIBLE);
        tvSlotsMessage.setVisibility(View.GONE);
        layoutSlots.removeAllViews();

        final String requestedDate = selectedDate;
        ApiClient.getService(this).getStationSlots(station.id, requestedDate)
                .enqueue(new Callback<List<Slot>>() {
                    @Override
                    public void onResponse(@NonNull Call<List<Slot>> call,
                                           @NonNull Response<List<Slot>> response) {
                        // Ignore answers for a day the user has already moved away from
                        if (isFinishing() || !requestedDate.equals(selectedDate)) {
                            return;
                        }
                        progressSlots.setVisibility(View.GONE);
                        if (response.isSuccessful() && response.body() != null) {
                            showSlots(response.body());
                        } else {
                            showSlotsMessage(getString(R.string.station_slots_error));
                        }
                    }

                    @Override
                    public void onFailure(@NonNull Call<List<Slot>> call, @NonNull Throwable t) {
                        if (isFinishing() || !requestedDate.equals(selectedDate)) {
                            return;
                        }
                        progressSlots.setVisibility(View.GONE);
                        showSlotsMessage(getString(R.string.station_slots_error));
                    }
                });
    }

    /**
     * Adds one row per time block with a usage bar and a free / full / closed pill.
     */
    private void showSlots(List<Slot> slots) {
        layoutSlots.removeAllViews();
        if (slots.isEmpty()) {
            showSlotsMessage(getString(R.string.station_slots_empty));
            return;
        }

        for (Slot slot : slots) {
            View row = LayoutInflater.from(this).inflate(R.layout.item_station_slot, layoutSlots, false);
            TextView tvTime = row.findViewById(R.id.tvSlotTime);
            TextView tvCount = row.findViewById(R.id.tvSlotCount);
            TextView tvSlotStatus = row.findViewById(R.id.tvSlotStatus);
            LinearProgressIndicator usage = row.findViewById(R.id.progressSlotUsage);

            tvTime.setText(DateUtils.formatLocalTime(slot.getStartTime())
                    + " – " + DateUtils.formatLocalTime(slot.getEndTime()));

            int total = Math.max(slot.getTotalSlots(), 0);
            int booked = Math.max(total - slot.getAvailableSlots(), 0);
            tvCount.setText(String.format(Locale.ENGLISH, "%d of %d battery slots booked", booked, total));
            usage.setProgress(total == 0 ? 0 : booked * 100 / total);

            String status;
            int textColor;
            int background;
            if (!slot.isAvailable()) {
                status = "Closed";
                textColor = R.color.text_secondary;
                background = R.color.navy_light;
            } else if (slot.getAvailableSlots() <= 0) {
                status = "Full";
                textColor = R.color.danger;
                background = R.color.danger_light;
            } else {
                status = slot.getAvailableSlots() + " free";
                textColor = R.color.success;
                background = R.color.success_light;
            }
            tvSlotStatus.setText(status);
            tvSlotStatus.setTextColor(ContextCompat.getColor(this, textColor));
            tvSlotStatus.setBackgroundTintList(ColorStateList.valueOf(ContextCompat.getColor(this, background)));
            usage.setIndicatorColor(ContextCompat.getColor(this,
                    slot.getAvailableSlots() <= 0 ? R.color.danger : R.color.solar));

            layoutSlots.addView(row);
        }
    }

    /**
     * Shows a message in place of the slot list (empty day or error).
     */
    private void showSlotsMessage(String message) {
        layoutSlots.removeAllViews();
        tvSlotsMessage.setText(message);
        tvSlotsMessage.setVisibility(View.VISIBLE);
    }

    /**
     * Opens the booking screen with this station already selected.
     */
    private void openCreateBooking() {
        Intent intent = new Intent(this, CreateBookingActivity.class);
        intent.putExtra(Constants.EXTRA_STATION_ID, station.id);
        startActivity(intent);
    }
}
