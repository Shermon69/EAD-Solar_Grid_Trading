/*
 * File:        BookingsActivity.java
 * Author:      Premaratne R.A.N.C (IT22050908)
 * Description: Shows Current/Pending bookings or Booking History, with a
 *              search box. The mode is chosen by the caller via an Intent extra.
 * Created:     29/09/2026
 */
package com.solargrid.app.activities;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.solargrid.app.R;
import com.solargrid.app.api.ApiClient;
import com.solargrid.app.models.Booking;

import java.util.ArrayList;
import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

/**
 * Screen that displays a list of bookings, either "current" (pending /
 * approved and in the future) or "history" (completed / cancelled or in
 * the past). A search box filters by station name or status.
 */
public class BookingsActivity extends AppCompatActivity {

    /** Intent extra key: "current" or "history". */
    public static final String EXTRA_TYPE = "type";

    private String type;
    private EditText etSearch;
    private TextView tvEmpty;
    private BookingAdapter adapter;

    /**
     * Sets up the list, search button and title, then loads the bookings.
     * Defaults to "current" if no type was passed in.
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_bookings);

        // Read the mode passed by the calling Activity.
        type = getIntent().getStringExtra(EXTRA_TYPE);
        if (type == null) type = "current";

        // Title depends on the mode.
        ((TextView) findViewById(R.id.tvTitle)).setText(
                type.equals("history")
                        ? "Booking History"
                        : "Current & Pending Bookings");

        // Bind views.
        etSearch = findViewById(R.id.etSearch);
        tvEmpty  = findViewById(R.id.tvEmpty);

        // Set up the RecyclerView with the adapter.
        adapter = new BookingAdapter();
        RecyclerView rv = findViewById(R.id.rvBookings);
        rv.setLayoutManager(new LinearLayoutManager(this));
        rv.setAdapter(adapter);

        // Search button and initial load.
        findViewById(R.id.btnSearch).setOnClickListener(v -> load());
        load();
    }

    /**
     * Loads bookings from the API using the search text. An empty search
     * box means "show all".
     */
    private void load() {
        String search = etSearch.getText().toString().trim();

        ApiClient.getService(this)
                .getBookings(type, search.isEmpty() ? null : search)
                .enqueue(new Callback<List<Booking>>() {

                    /** Updates the adapter and toggles the empty-state message. */
                    @Override
                    public void onResponse(Call<List<Booking>> call,
                                           Response<List<Booking>> response) {
                        List<Booking> list = (response.isSuccessful() && response.body() != null)
                                ? response.body()
                                : new ArrayList<>();

                        adapter.setItems(list);
                        tvEmpty.setVisibility(list.isEmpty() ? View.VISIBLE : View.GONE);
                    }

                    /** Handles network errors. */
                    @Override
                    public void onFailure(Call<List<Booking>> call, Throwable t) {
                        Toast.makeText(BookingsActivity.this,
                                "Cannot reach the server.",
                                Toast.LENGTH_LONG).show();
                    }
                });
    }

    /**
     * RecyclerView adapter that renders one booking per row. Kept as a
     * static inner class so it does not capture the Activity.
     */
    private static class BookingAdapter extends RecyclerView.Adapter<BookingAdapter.Holder> {

        private List<Booking> items = new ArrayList<>();

        /** Replaces the list with new items and refreshes the screen. */
        void setItems(List<Booking> list) {
            items = list;
            notifyDataSetChanged();
        }

        /** Creates the row view from the item_booking layout. */
        @Override
        public Holder onCreateViewHolder(ViewGroup parent, int viewType) {
            View v = LayoutInflater.from(parent.getContext())
                    .inflate(R.layout.item_booking, parent, false);
            return new Holder(v);
        }

        /** Fills the row's TextViews with one booking's data. */
        @Override
        public void onBindViewHolder(Holder h, int position) {
            Booking b = items.get(position);

            h.station.setText(b.stationName);

            // Show only the first 10 characters of the ISO date (yyyy-MM-dd).
            String date = (b.reservationDate == null)
                    ? ""
                    : b.reservationDate.substring(0, Math.min(10, b.reservationDate.length()));

            h.details.setText(date + "  |  " + b.status);
        }

        /** Number of rows in the list. */
        @Override
        public int getItemCount() {
            return items.size();
        }

        /** Holds the row's view references to avoid repeated findViewById calls. */
        static class Holder extends RecyclerView.ViewHolder {
            TextView station, details;

            Holder(View v) {
                super(v);
                station = v.findViewById(R.id.tvStation);
                details = v.findViewById(R.id.tvDetails);
            }
        }
    }
}