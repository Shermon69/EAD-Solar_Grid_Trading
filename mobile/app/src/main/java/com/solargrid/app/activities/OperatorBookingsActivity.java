/*
 * File:        OperatorBookingsActivity.java
 * Author:      Shermon H (IT22177964)
 * Description: Operator mode screen. Lists prosumer bookings from the Web API
 *              with a status filter and an NIC search, and lets a grid operator
 *              approve pending bookings or cancel them. All rules (for example
 *              the 12-hour cancel rule) are checked by the API; this screen only
 *              shows the result or the API's error message.
 */

package com.solargrid.app.activities;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.view.inputmethod.EditorInfo;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.content.ContextCompat;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import com.google.android.material.button.MaterialButton;
import com.google.android.material.chip.ChipGroup;
import com.google.android.material.textfield.TextInputEditText;
import com.solargrid.app.R;
import com.solargrid.app.api.ApiClient;
import com.solargrid.app.api.ApiService;
import com.solargrid.app.models.Reservation;
import com.solargrid.app.utils.ApiErrorUtils;
import com.solargrid.app.utils.Constants;
import com.solargrid.app.utils.DateUtils;

import java.util.ArrayList;
import java.util.List;
import java.util.Locale;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

/**
 * Bookings list for grid operators, with Approve and Cancel actions.
 */
public class OperatorBookingsActivity extends AppCompatActivity {

    private ApiService apiService;
    private BookingAdapter adapter;

    private ChipGroup chipGroupStatus;
    private TextInputEditText etSearchNic;
    private ProgressBar progressBookings;
    private TextView tvNoBookings;

    // True while an approve/cancel call is running, so a double tap is ignored
    private boolean actionInProgress = false;

    /**
     * Sets up the list, the status chips and the NIC search box, then loads the bookings.
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_operator_bookings);

        apiService = ApiClient.getService(this);

        chipGroupStatus = findViewById(R.id.chipGroupStatus);
        etSearchNic = findViewById(R.id.etSearchNic);
        progressBookings = findViewById(R.id.progressBookings);
        tvNoBookings = findViewById(R.id.tvNoBookings);

        adapter = new BookingAdapter(new BookingAdapter.Listener() {
            /**
             * Called when the operator taps Approve on a booking.
             */
            @Override
            public void onApprove(Reservation reservation) {
                approveBooking(reservation);
            }

            /**
             * Called when the operator taps Cancel on a booking.
             */
            @Override
            public void onCancel(Reservation reservation) {
                confirmCancel(reservation);
            }
        });

        RecyclerView recycler = findViewById(R.id.recyclerBookings);
        recycler.setLayoutManager(new LinearLayoutManager(this));
        recycler.setAdapter(adapter);

        // Reload whenever a different status chip is chosen
        chipGroupStatus.setOnCheckedStateChangeListener((group, checkedIds) -> loadBookings());

        // Reload when the search key on the keyboard is pressed
        etSearchNic.setOnEditorActionListener((view, actionId, event) -> {
            if (actionId == EditorInfo.IME_ACTION_SEARCH) {
                loadBookings();
                return true;
            }
            return false;
        });
    }

    /**
     * Reloads the list every time the screen is shown, so it is never out of date.
     */
    @Override
    protected void onResume() {
        super.onResume();
        loadBookings();
    }

    /**
     * Returns the status chosen in the chips (e.g. "Pending"), or null for "All".
     */
    private String selectedStatus() {
        int id = chipGroupStatus.getCheckedChipId();

        if (id == R.id.chipPending) {
            return Constants.STATUS_PENDING;
        }
        if (id == R.id.chipApproved) {
            return Constants.STATUS_APPROVED;
        }
        if (id == R.id.chipCompleted) {
            return Constants.STATUS_COMPLETED;
        }
        if (id == R.id.chipCancelled) {
            return Constants.STATUS_CANCELLED;
        }
        return null;
    }

    /**
     * Asks the API for the bookings that match the chosen status and NIC, then shows them.
     */
    private void loadBookings() {
        String nic = etSearchNic.getText() == null
                ? ""
                : etSearchNic.getText().toString().trim().toUpperCase(Locale.ROOT);

        progressBookings.setVisibility(View.VISIBLE);
        tvNoBookings.setVisibility(View.GONE);

        apiService.getAllReservations(selectedStatus(), null, nic.isEmpty() ? null : nic)
                .enqueue(new Callback<List<Reservation>>() {
                    /**
                     * Called when the API answers; shows the list or the error message.
                     */
                    @Override
                    public void onResponse(@NonNull Call<List<Reservation>> call,
                                           @NonNull Response<List<Reservation>> response) {
                        progressBookings.setVisibility(View.GONE);

                        if (!response.isSuccessful() || response.body() == null) {
                            adapter.setItems(new ArrayList<>());
                            Toast.makeText(OperatorBookingsActivity.this,
                                    ApiErrorUtils.getMessage(response), Toast.LENGTH_LONG).show();
                            return;
                        }

                        adapter.setItems(response.body());
                        tvNoBookings.setVisibility(response.body().isEmpty() ? View.VISIBLE : View.GONE);
                    }

                    /**
                     * Called when the API cannot be reached.
                     */
                    @Override
                    public void onFailure(@NonNull Call<List<Reservation>> call, @NonNull Throwable t) {
                        progressBookings.setVisibility(View.GONE);
                        Toast.makeText(OperatorBookingsActivity.this,
                                ApiErrorUtils.getNetworkMessage(t), Toast.LENGTH_LONG).show();
                    }
                });
    }

    /**
     * Sends the approve request. The API checks the booking is still pending and creates its QR token.
     */
    private void approveBooking(Reservation reservation) {
        if (actionInProgress) {
            return;
        }
        actionInProgress = true;

        apiService.approveReservation(reservation.getId()).enqueue(new Callback<Reservation>() {
            /**
             * Called when the API answers the approve request.
             */
            @Override
            public void onResponse(@NonNull Call<Reservation> call, @NonNull Response<Reservation> response) {
                finishAction(response, R.string.booking_approved);
            }

            /**
             * Called when the API cannot be reached.
             */
            @Override
            public void onFailure(@NonNull Call<Reservation> call, @NonNull Throwable t) {
                actionInProgress = false;
                Toast.makeText(OperatorBookingsActivity.this,
                        ApiErrorUtils.getNetworkMessage(t), Toast.LENGTH_LONG).show();
            }
        });
    }

    /**
     * Asks the operator to confirm before a booking is cancelled.
     */
    private void confirmCancel(Reservation reservation) {
        new AlertDialog.Builder(this)
                .setTitle(R.string.cancel_confirm_title)
                .setMessage(reservation.getStationName() + "\n"
                        + DateUtils.formatLocal(reservation.getReservationTime()) + "\n"
                        + reservation.getProsumerNic())
                .setPositiveButton(R.string.cancel_confirm_yes, (dialog, which) -> cancelBooking(reservation))
                .setNegativeButton(R.string.cancel_confirm_no, null)
                .show();
    }

    /**
     * Sends the cancel request. The API enforces the 12-hour rule and frees the slot.
     */
    private void cancelBooking(Reservation reservation) {
        if (actionInProgress) {
            return;
        }
        actionInProgress = true;

        apiService.cancelReservation(reservation.getId()).enqueue(new Callback<Reservation>() {
            /**
             * Called when the API answers the cancel request.
             */
            @Override
            public void onResponse(@NonNull Call<Reservation> call, @NonNull Response<Reservation> response) {
                finishAction(response, R.string.booking_cancelled);
            }

            /**
             * Called when the API cannot be reached.
             */
            @Override
            public void onFailure(@NonNull Call<Reservation> call, @NonNull Throwable t) {
                actionInProgress = false;
                Toast.makeText(OperatorBookingsActivity.this,
                        ApiErrorUtils.getNetworkMessage(t), Toast.LENGTH_LONG).show();
            }
        });
    }

    /**
     * Shows the result of an approve/cancel call (success message or the API's
     * error text, e.g. the 12-hour rule) and refreshes the list.
     */
    private void finishAction(Response<Reservation> response, int successMessage) {
        actionInProgress = false;

        if (response.isSuccessful()) {
            Toast.makeText(this, successMessage, Toast.LENGTH_SHORT).show();
        } else {
            Toast.makeText(this, ApiErrorUtils.getMessage(response), Toast.LENGTH_LONG).show();
        }
        loadBookings();
    }

    /**
     * RecyclerView adapter that shows one card per booking.
     */
    private static class BookingAdapter extends RecyclerView.Adapter<BookingAdapter.BookingHolder> {

        /**
         * Receives the taps on the Approve and Cancel buttons.
         */
        interface Listener {

            /**
             * Called when Approve is tapped.
             */
            void onApprove(Reservation reservation);

            /**
             * Called when Cancel is tapped.
             */
            void onCancel(Reservation reservation);
        }

        private final Listener listener;
        private List<Reservation> items = new ArrayList<>();

        /**
         * Creates the adapter with the object that handles button taps.
         */
        BookingAdapter(Listener listener) {
            this.listener = listener;
        }

        /**
         * Replaces the list shown and redraws it.
         */
        void setItems(List<Reservation> newItems) {
            items = newItems;
            notifyDataSetChanged();
        }

        /**
         * Creates the view for one booking card.
         */
        @NonNull
        @Override
        public BookingHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
            View view = LayoutInflater.from(parent.getContext())
                    .inflate(R.layout.item_operator_booking, parent, false);
            return new BookingHolder(view);
        }

        /**
         * Fills one card. Approve shows only for Pending bookings and Cancel only for
         * Pending or Approved ones; Completed and Cancelled bookings have no actions.
         */
        @Override
        public void onBindViewHolder(@NonNull BookingHolder holder, int position) {
            Reservation r = items.get(position);
            String status = r.getStatus() == null ? "" : r.getStatus();

            holder.tvStation.setText(r.getStationName());
            holder.tvTime.setText(DateUtils.formatLocal(r.getReservationTime()));
            holder.tvProsumer.setText("Prosumer NIC: " + r.getProsumerNic());
            holder.tvDetails.setText(String.format(Locale.ENGLISH, "%s  |  %.1f kWh", r.getType(), r.getEnergyKwh()));

            holder.tvStatus.setText(status);
            holder.tvStatus.setTextColor(ContextCompat.getColor(holder.itemView.getContext(), colorFor(status)));

            boolean pending = Constants.STATUS_PENDING.equals(status);
            boolean approved = Constants.STATUS_APPROVED.equals(status);

            holder.btnApprove.setVisibility(pending ? View.VISIBLE : View.GONE);
            holder.btnCancel.setVisibility(pending || approved ? View.VISIBLE : View.GONE);
            holder.layoutActions.setVisibility(pending || approved ? View.VISIBLE : View.GONE);

            holder.btnApprove.setOnClickListener(v -> listener.onApprove(r));
            holder.btnCancel.setOnClickListener(v -> listener.onCancel(r));
        }

        /**
         * Returns how many bookings are in the list.
         */
        @Override
        public int getItemCount() {
            return items.size();
        }

        /**
         * Picks the colour of the status text.
         */
        private static int colorFor(String status) {
            switch (status) {
                case Constants.STATUS_APPROVED:
                    return R.color.info;
                case Constants.STATUS_COMPLETED:
                    return R.color.success;
                case Constants.STATUS_CANCELLED:
                    return R.color.danger;
                default:
                    return R.color.solar;
            }
        }

        /**
         * Holds the views of one booking card.
         */
        static class BookingHolder extends RecyclerView.ViewHolder {

            final TextView tvStation;
            final TextView tvStatus;
            final TextView tvTime;
            final TextView tvProsumer;
            final TextView tvDetails;
            final View layoutActions;
            final MaterialButton btnApprove;
            final MaterialButton btnCancel;

            /**
             * Finds the views inside one booking card.
             */
            BookingHolder(@NonNull View itemView) {
                super(itemView);
                tvStation = itemView.findViewById(R.id.tvStation);
                tvStatus = itemView.findViewById(R.id.tvStatus);
                tvTime = itemView.findViewById(R.id.tvTime);
                tvProsumer = itemView.findViewById(R.id.tvProsumer);
                tvDetails = itemView.findViewById(R.id.tvDetails);
                layoutActions = itemView.findViewById(R.id.layoutActions);
                btnApprove = itemView.findViewById(R.id.btnApprove);
                btnCancel = itemView.findViewById(R.id.btnCancel);
            }
        }
    }
}
