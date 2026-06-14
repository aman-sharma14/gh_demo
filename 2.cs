// ═══════════════════════════════════════════════════════════════════
// HOSPITAL MANAGEMENT SYSTEM
// 
// Design decisions:
//   - SRP: Each class has one responsibility
//   - OCP: New doctor types can be added without changing existing code
//   - LSP: FullTimeDoctor and VisitingConsultant can replace Doctor anywhere
//   - DIP: Clinic depends on abstractions (Doctor, IPayable) not concretions
//   - High cohesion: Each class contains only related behaviour
//   - Low coupling: Classes communicate through abstractions and events
//   - Records: For immutable data (MedicalRecord, AppointmentSummary)
//   - Delegates/Events: Clinic doesn't know who handles notifications
//   - Custom exceptions: Specific, meaningful error types
//   - LINQ: All reporting done via LINQ, no manual loops
// ═══════════════════════════════════════════════════════════════════

namespace HospitalManagement
{
    // ── Custom Exceptions ─────────────────────────────────────────
    // SRP: Each exception describes exactly one error condition

    class DuplicateAppointmentException : Exception
    {
        public DuplicateAppointmentException(string message) : base(message) { }
    }

    class InvalidAppointmentOperationException : Exception
    {
        public InvalidAppointmentOperationException(string message) : base(message) { }
    }

    class DoctorNotFoundException : Exception
    {
        public DoctorNotFoundException(string message) : base(message) { }
    }

    // ── Enums ─────────────────────────────────────────────────────

    enum AppointmentStatus { Scheduled, Completed, Cancelled }

    enum Specialization { General, Cardiology, Neurology, Orthopedics }

    // ── Records — immutable data containers ───────────────────────
    // Records are perfect here — medical history and summaries
    // should never be mutated after creation

    record MedicalRecord(string Diagnosis, DateTime Date, string DoctorName);

    record AppointmentSummary(
        string PatientName,
        string DoctorName,
        DateTime AppointmentDate,
        AppointmentStatus Status,
        decimal Fee);

    // ── Delegates ─────────────────────────────────────────────────
    // Clinic decides at runtime what happens on book/cancel
    // The appointment system fires the event — it doesn't care who listens

    delegate void AppointmentEventHandler(AppointmentSummary summary);

    // ── Interface ─────────────────────────────────────────────────
    // IPayable — anything that can be paid implements this
    // DIP: Clinic uses IPayable, not concrete doctor types

    interface IPayable
    {
        decimal CalculatePay(int appointmentsCompleted);
    }

    // ── Abstract Base Class ───────────────────────────────────────
    // Doctor — shared identity and behaviour
    // Abstract because you never instantiate a plain "Doctor"

    abstract class Doctor : IPayable
    {
        // init — identity never changes after construction
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public Specialization Specialization { get; init; }
        public decimal ConsultationFee { get; init; }

        protected Doctor(int id, string name, Specialization specialization, decimal consultationFee)
        {
            Id = id;
            Name = name;
            Specialization = specialization;
            ConsultationFee = consultationFee;
        }

        // Each subclass knows how to calculate its own pay — polymorphism
        public abstract decimal CalculatePay(int appointmentsCompleted);

        public override string ToString()
            => $"Dr. {Name} ({Specialization}) — Fee: {ConsultationFee:C}";
    }

    // FullTimeDoctor — salaried + bonus per completed appointment
    class FullTimeDoctor : Doctor
    {
        public decimal MonthlySalary { get; init; }
        private const decimal BonusPerAppointment = 500m;

        public FullTimeDoctor(int id, string name, Specialization specialization,
            decimal consultationFee, decimal monthlySalary)
            : base(id, name, specialization, consultationFee)
        {
            MonthlySalary = monthlySalary;
        }

        public override decimal CalculatePay(int appointmentsCompleted)
            => MonthlySalary + (appointmentsCompleted * BonusPerAppointment);
    }

    // VisitingConsultant — paid per appointment only, no salary
    class VisitingConsultant : Doctor
    {
        public decimal RatePerAppointment { get; init; }

        public VisitingConsultant(int id, string name, Specialization specialization,
            decimal consultationFee, decimal ratePerAppointment)
            : base(id, name, specialization, consultationFee)
        {
            RatePerAppointment = ratePerAppointment;
        }

        public override decimal CalculatePay(int appointmentsCompleted)
            => appointmentsCompleted * RatePerAppointment;
    }

    // ── Patient ───────────────────────────────────────────────────
    // SRP: Patient manages its own identity and medical history only
    // It does not know about appointments or the clinic

    class Patient
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public int Age { get; init; }
        public string ContactNumber { get; init; } = string.Empty;

        // Private — only Patient controls its own history
        private readonly List<MedicalRecord> _medicalHistory = new();

        // Read-only view — outside code can read but not modify
        public IReadOnlyList<MedicalRecord> MedicalHistory => _medicalHistory;

        public Patient(int id, string name, int age, string contactNumber)
        {
            Id = id;
            Name = name;
            Age = age;
            ContactNumber = contactNumber;
        }

        public void AddMedicalRecord(MedicalRecord record)
            => _medicalHistory.Add(record);

        public override string ToString()
            => $"{Name} (Age: {Age}) — {_medicalHistory.Count} record(s)";
    }

    // ── Appointment ───────────────────────────────────────────────
    // SRP: Appointment manages its own state transitions only
    // It fires events but doesn't know what happens after

    class Appointment
    {
        public int Id { get; init; }
        public Patient Patient { get; init; } = null!;
        public Doctor Doctor { get; init; } = null!;
        public DateTime AppointmentDate { get; init; }
        public decimal Fee { get; private set; }

        // Private set — status only changes through controlled methods
        public AppointmentStatus Status { get; private set; } = AppointmentStatus.Scheduled;

        // Events — Appointment announces what happened, clinic decides what to do
        public event AppointmentEventHandler? OnBooked;
        public event AppointmentEventHandler? OnCancelled;
        public event AppointmentEventHandler? OnCompleted;

        public Appointment(int id, Patient patient, Doctor doctor, DateTime date)
        {
            Id = id;
            Patient = patient;
            Doctor = doctor;
            AppointmentDate = date;
            Fee = doctor.ConsultationFee; // fee set at booking based on doctor type
        }

        public void Book()
        {
            // Fire event — whoever subscribed handles the notification
            OnBooked?.Invoke(ToSummary());
        }

        public void Cancel()
        {
            if (Status == AppointmentStatus.Cancelled)
                throw new InvalidAppointmentOperationException(
                    $"Appointment {Id} is already cancelled.");

            if (Status == AppointmentStatus.Completed)
                throw new InvalidAppointmentOperationException(
                    $"Completed appointment {Id} cannot be cancelled.");

            Status = AppointmentStatus.Cancelled;
            Fee = 0m; // no charge on cancellation
            OnCancelled?.Invoke(ToSummary());
        }

        public void Complete()
        {
            if (Status != AppointmentStatus.Scheduled)
                throw new InvalidAppointmentOperationException(
                    $"Appointment {Id} cannot be completed — current status: {Status}");

            Status = AppointmentStatus.Completed;
            Patient.AddMedicalRecord(new MedicalRecord(
                $"Consultation with Dr. {Doctor.Name}",
                AppointmentDate,
                Doctor.Name));

            OnCompleted?.Invoke(ToSummary());
        }

        // Private helper — builds summary record for event payload
        private AppointmentSummary ToSummary()
            => new(Patient.Name, Doctor.Name, AppointmentDate, Status, Fee);

        public override string ToString()
            => $"[{Id}] {Patient.Name} → Dr. {Doctor.Name} on {AppointmentDate:dd MMM yyyy} [{Status}]";
    }

    // ── Clinic ────────────────────────────────────────────────────
    // SRP: Clinic manages the collection of doctors, patients, appointments
    // It wires up events and enforces business rules
    // Low coupling: depends on Doctor (abstract) and Patient, not concrete types

    class Clinic
    {
        private readonly Dictionary<int, Doctor> _doctors = new();
        private readonly Dictionary<int, Patient> _patients = new();
        private readonly List<Appointment> _appointments = new();
        private int _nextAppointmentId = 1;

        // Events at clinic level — subscribers attach here once
        // Clinic subscribes its own handlers to each appointment's events
        public event AppointmentEventHandler? OnAppointmentBooked;
        public event AppointmentEventHandler? OnAppointmentCancelled;
        public event AppointmentEventHandler? OnAppointmentCompleted;

        public void AddDoctor(Doctor doctor) => _doctors[doctor.Id] = doctor;
        public void AddPatient(Patient patient) => _patients[patient.Id] = patient;

        public Appointment BookAppointment(int patientId, int doctorId, DateTime date)
        {
            // Validate doctor exists
            if (!_doctors.TryGetValue(doctorId, out var doctor))
                throw new DoctorNotFoundException($"Doctor with ID {doctorId} not found.");

            if (!_patients.TryGetValue(patientId, out var patient))
                throw new DoctorNotFoundException($"Patient with ID {patientId} not found.");

            // Duplicate check — same patient, same doctor, same date
            bool duplicate = _appointments.Any(a =>
                a.Patient.Id == patientId &&
                a.Doctor.Id == doctorId &&
                a.AppointmentDate.Date == date.Date &&
                a.Status != AppointmentStatus.Cancelled);

            if (duplicate)
                throw new DuplicateAppointmentException(
                    $"{patient.Name} already has an appointment with Dr. {doctor.Name} on {date:dd MMM yyyy}.");

            var appointment = new Appointment(_nextAppointmentId++, patient, doctor, date);

            // Wire up appointment events to clinic-level events
            // Low coupling — appointment doesn't know about clinic's handlers
            appointment.OnBooked += summary => OnAppointmentBooked?.Invoke(summary);
            appointment.OnCancelled += summary => OnAppointmentCancelled?.Invoke(summary);
            appointment.OnCompleted += summary => OnAppointmentCompleted?.Invoke(summary);

            _appointments.Add(appointment);
            appointment.Book();

            return appointment;
        }

        public void CancelAppointment(int appointmentId)
        {
            var appointment = GetAppointmentOrThrow(appointmentId);
            appointment.Cancel();
        }

        public void CompleteAppointment(int appointmentId)
        {
            var appointment = GetAppointmentOrThrow(appointmentId);
            appointment.Complete();
        }

        // ── LINQ Reporting ────────────────────────────────────────
        // SRP: Reporting is Clinic's job — it has access to all data

        public decimal GetTotalRevenue()
            => _appointments
                .Where(a => a.Status == AppointmentStatus.Completed)
                .Sum(a => a.Fee);

        public Dictionary<string, int> GetAppointmentsPerDoctor()
            => _appointments
                .GroupBy(a => a.Doctor.Name)
                .ToDictionary(g => g.Key, g => g.Count());

        public List<Patient> GetPatientsWithMultipleVisits()
            => _appointments
                .Where(a => a.Status == AppointmentStatus.Completed)
                .GroupBy(a => a.Patient.Id)
                .Where(g => g.Count() > 1)
                .Select(g => g.First().Patient)
                .ToList();

        public Doctor GetBusiestDoctor()
            => _appointments
                .GroupBy(a => a.Doctor)
                .OrderByDescending(g => g.Count())
                .First().Key;

        public List<Appointment> GetAppointmentsByStatus(AppointmentStatus status)
            => _appointments.Where(a => a.Status == status).ToList();

        public List<Appointment> GetAppointmentsByDateRange(DateTime from, DateTime to)
            => _appointments
                .Where(a => a.AppointmentDate.Date >= from.Date &&
                            a.AppointmentDate.Date <= to.Date)
                .ToList();

        public decimal GetDoctorPay(int doctorId)
        {
            if (!_doctors.TryGetValue(doctorId, out var doctor))
                throw new DoctorNotFoundException($"Doctor {doctorId} not found.");

            int completed = _appointments.Count(a =>
                a.Doctor.Id == doctorId &&
                a.Status == AppointmentStatus.Completed);

            // Polymorphism — CalculatePay behaves differently per type
            // No type checking needed — LSP in action
            return doctor.CalculatePay(completed);
        }

        public List<T> GetDoctorsByType<T>() where T : Doctor
            => _doctors.Values.OfType<T>().ToList();

        // Private helper — DRY
        private Appointment GetAppointmentOrThrow(int id)
        {
            var appointment = _appointments.FirstOrDefault(a => a.Id == id);
            if (appointment == null)
                throw new InvalidAppointmentOperationException($"Appointment {id} not found.");
            return appointment;
        }
    }

    // ── Program ───────────────────────────────────────────────────

    internal class Program
    {
        static void Main(string[] args)
        {
            var clinic = new Clinic();

            // Subscribe to clinic-level events — multicast delegates
            // Audit log handler
            clinic.OnAppointmentBooked += summary =>
                Console.WriteLine($"[AUDIT] Booked: {summary.PatientName} → {summary.DoctorName} on {summary.AppointmentDate:dd MMM}");

            // Alert handler
            clinic.OnAppointmentBooked += summary =>
                Console.WriteLine($"[ALERT] New appointment scheduled. Fee: {summary.Fee:C}");

            clinic.OnAppointmentCancelled += summary =>
                Console.WriteLine($"[AUDIT] Cancelled: {summary.PatientName}'s appointment with {summary.DoctorName}");

            clinic.OnAppointmentCompleted += summary =>
                Console.WriteLine($"[AUDIT] Completed: {summary.PatientName} visited {summary.DoctorName}");

            // Setup doctors
            var drSmith = new FullTimeDoctor(1, "Smith", Specialization.Cardiology, 1500m, 80000m);
            var drPatel = new FullTimeDoctor(2, "Patel", Specialization.Neurology, 2000m, 90000m);
            var drKhan = new VisitingConsultant(3, "Khan", Specialization.Orthopedics, 1200m, 3000m);

            clinic.AddDoctor(drSmith);
            clinic.AddDoctor(drPatel);
            clinic.AddDoctor(drKhan);

            // Setup patients
            var alice = new Patient(1, "Alice", 30, "9999999999");
            var bob = new Patient(2, "Bob", 45, "8888888888");
            var carol = new Patient(3, "Carol", 28, "7777777777");

            clinic.AddPatient(alice);
            clinic.AddPatient(bob);
            clinic.AddPatient(carol);

            Console.WriteLine("\n── Booking Appointments ──");

            Appointment a1 = null!, a2 = null!, a3 = null!, a4 = null!;

            try { a1 = clinic.BookAppointment(1, 1, DateTime.Today); }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            try { a2 = clinic.BookAppointment(2, 1, DateTime.Today); }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            try { a3 = clinic.BookAppointment(1, 2, DateTime.Today); }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            try { a4 = clinic.BookAppointment(3, 3, DateTime.Today); }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            Console.WriteLine("\n── Duplicate booking attempt ──");
            try { clinic.BookAppointment(1, 1, DateTime.Today); }
            catch (DuplicateAppointmentException ex) { Console.WriteLine($"Caught: {ex.Message}"); }

            Console.WriteLine("\n── Completing and Cancelling ──");
            try { clinic.CompleteAppointment(a1.Id); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { clinic.CancelAppointment(a2.Id); } catch (Exception ex) { Console.WriteLine(ex.Message); }

            Console.WriteLine("\n── Cancel already cancelled ──");
            try { clinic.CancelAppointment(a2.Id); }
            catch (InvalidAppointmentOperationException ex) { Console.WriteLine($"Caught: {ex.Message}"); }

            Console.WriteLine("\n── Cancel completed appointment ──");
            try { clinic.CancelAppointment(a1.Id); }
            catch (InvalidAppointmentOperationException ex) { Console.WriteLine($"Caught: {ex.Message}"); }

            Console.WriteLine("\n── Reports ──");
            Console.WriteLine($"Total Revenue: {clinic.GetTotalRevenue():C}");

            Console.WriteLine("\nAppointments per doctor:");
            foreach (var (name, count) in clinic.GetAppointmentsPerDoctor())
                Console.WriteLine($"  Dr. {name}: {count}");

            Console.WriteLine($"\nBusiest doctor: Dr. {clinic.GetBusiestDoctor().Name}");

            Console.WriteLine("\nScheduled appointments:");
            clinic.GetAppointmentsByStatus(AppointmentStatus.Scheduled)
                  .ForEach(a => Console.WriteLine($"  {a}"));

            Console.WriteLine("\n── Doctor Pay ──");
            Console.WriteLine($"Dr. Smith pay: {clinic.GetDoctorPay(1):C}");
            Console.WriteLine($"Dr. Khan pay:  {clinic.GetDoctorPay(3):C}");

            Console.WriteLine("\n── Visiting Consultants ──");
            clinic.GetDoctorsByType<VisitingConsultant>()
                  .ForEach(d => Console.WriteLine($"  {d}"));

            Console.WriteLine("\n── Patients with multiple visits ──");
            var repeat = clinic.GetPatientsWithMultipleVisits();
            if (repeat.Any())
                repeat.ForEach(p => Console.WriteLine($"  {p}"));
            else
                Console.WriteLine("  None yet.");
        }
    }
}