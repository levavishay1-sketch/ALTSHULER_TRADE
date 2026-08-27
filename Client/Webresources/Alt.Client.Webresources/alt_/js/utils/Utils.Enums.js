const INTERNAL_SERVER_ERROR = "שגיאה פנימית. נא לפנות למנהל מערכת.";
const LINK_REQUEST_NOTIFICATION_MESSAGE = "המערכת מבצעת בקשה לקבלת לינק לטופס דיגיטלי...";
const DATA_CONSTRUCTION_NOTIFICATION_MESSAGE = "המערכת מבצעת קליטת מתונים למערכת...";
const SENDING_NOW_SMS_NOTIFICATION_MESSAGE = "המערכת מבצעת שליחה...";

var entityName = {
    Lead: "lead",
    Opportunity: "opportunity",
    Contact: "contact",
    Account: "account",
    Incident: "incident",
    Portfolio: "alt_portfolio",
    DigitalFormVerification: "alt_digitalformverification"
};

var entityTypeCode = {
    Account: 1,
    Contact: 2,
    Lead: 4,
    Incident: 112
};

var identityTypeCode = {
    GovernmentId: 1,
    CompanyNumber: 2
};

var leadStateCode = {
    Active: 0,
    Qualified: 1,
    Disqualified: 2,
};

var customEntityStateCode = {
    Active: 0,
    Inactive: 1
};

var yesNoCode = { //not BIT field
    No: 0,
    Yes: 1
};

var notificationLevel = {
    Warning: "WARNING",
    Error: "ERROR",
    Info: "INFO"
};

var activityTemplateType = {
    Sms: 0,
    Email: 1
};

var smsStatus = {
    Draft: 1,
    Send: 100000000,
    SendingNow: 100000001,
    SentSuccessfully: 2,
    Faild: 100000003
};

var digitalFormStatusCode = {
    Draft: 1,
    Copleted: 2,
    SentToVerification: 455710000,
    Canceld: 3,
    Send: 100000000,
    Sending: 100000001,
    Sent: 100000002,
    InProgress: 100000003,
    Faild: 100000005
};

var activityStatus = {
    Draft: 1
};

var LogMessageLevel = {
    Information: 1,
    Warning: 2,
    Error: 3,
    Critical: 4
};

var SaveMode = {
    Save: 1,
    SaveAndClose: 2,
    Deactivate: 5,
    Reactivate: 6,
    Send: 7,
    Disqualify: 15,
    Qualify: 16,
    Assign: 47,
    SaveAsCompleted: 58,
    SaveAndNew: 59,
    AutoSave: 70
};

var SubmitMode = {
    Always: 'always',
    Never: 'never',
    Dirty: 'dirty' //default
};

var incidentStateCodes = {
    Active: 0,
    Resolved: 1,
    Canceled: 2
};

var incidentStatusCodes = {
    OnGoing: 1,
    Holding: 2,
    WaitingForDetails: 3,
    Checking: 4,
    Solved: 5,
    Cancelled: 6,
    InformationProvided: 1000,
    Merged: 2000
};

var dataReceptionStatusCode = {
    UnderConstruction: 1,
    Success: 2,
    Faild: 3,
    Retry: 4
};
var transferStatusCode = {
    Waiting: 1,
    Send: 2,
    Sending: 3,
    Sent: 4,
    Faild: 5
};

