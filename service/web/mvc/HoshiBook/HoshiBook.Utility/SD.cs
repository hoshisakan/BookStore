﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoshiBook.Utility;

public static class SD
{
    public const string Role_User_Indi = "Individual";
    public const string Role_User_Comp = "Company";
    public const string Role_Admin = "Admin";
    public const string Role_Employee = "Employee";

    public const string StatusPending = "Pending";
    public const string StatusApproved = "Approved";
    public const string StatusInProgress = "Processing";
    public const string StatusShipped = "Shipped";
    public const string StatusCancelled = "Cancelled";
    public const string StatusRefunded = "Refunded";

    public const string PaymentStatusPending = "Pending";
    public const string PaymentStatusApproved = "Approved";
    public const string PaymentStatusDelayed = "ApprovedForDelayedPayment";
    public const string PaymentStatusRejected = "Rejected";

}
