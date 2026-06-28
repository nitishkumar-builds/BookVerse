namespace BookVerse.Utilities
{
    /// <summary>
    /// Static Details — application-wide constants.
    /// </summary>
    public static class SD
    {
        // ── Roles ────────────────────────────────────────────────────────────────
        public const string Role_Admin = "Admin";
        public const string Role_Employee = "Employee";
        public const string Role_Company = "Company";
        public const string Role_Customer = "Customer";

        // ── Order Status ─────────────────────────────────────────────────────────
        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusInProcess = "Processing";
        public const string StatusShipped = "Shipped";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";

        // ── Payment Status ───────────────────────────────────────────────────────
        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusApprovedForDelayedPayment = "ApprovedForDelayedPayment"; public const string PaymentStatusRejected = "Rejected";
    }
}