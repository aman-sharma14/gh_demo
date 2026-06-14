namespace HospitalManagement_System
{
    class InvalidOperationException : Exception
    {
        public InvalidOperationException() { }
        public InvalidOperationException(string message) : base(message) { }
        public InvalidOperationException(string message, Exception innerException) : base(message, innerException) { }
    abstract class Doctor
    {
        public string Name { get; set; }
        public string Specialisation { get; set; }
        public decimal fee { get; set; }
        public int Appointments { get; set; }

        public Doctor(string Name, string Spec, decimal fee)
        {
            this.Name = Name;
            this.Specialisation = Spec;
            this.fee= fee;
            this.Appointments = 0;
        }

        public virtual decimal GetTotalPay()
        {
            return Appointments * fee;
        }
         
    }

    class FullTimeDoctor : Doctor
    {
        public FullTimeDoctor(string Name, string Spec, decimal fee, decimal sal) : base(Name, Spec, fee)
        {
            this.Salary = sal;
        }

        public decimal Salary { get; set; }
        
        public decimal Bonus()
        {
            return Appointments * 25;
        }

        public override decimal GetTotalPay()
        {
            return base.GetTotalPay() + Salary + Bonus();
        }
    }

    class ContractDoctor : Doctor
    {
        public ContractDoctor(string Name, string Spec, decimal fee) : base(Name, Spec, fee)
        {
        }
    }

    class Patient
    {
        public string name { get; set; }
        public int age { get; set; }
        public int contact {  get; set; }

        List<Diagnosis> diagnoses;

        public Patient(string n, int a, int c)
            {
                this.name = n;
                this.age = a;
                this.contact = c;
                this.diagnoses = new List<Diagnosis>();
            }

        public void AddDiagnosis(Diagnosis diagnosis)
            {
                diagnoses.Add(diagnosis);
            }
    }

        public record Diagnosis(string docName, string issue); //wanted to add doctor as object but some accessibilty error same below
    //public record AppointmentRecord(Doctor doctor, Patient patient, DateTime enrolledOn);

    class Appointment
    {
        public int id { get; init; }
        public Doctor doctor { get; init; }
        public Patient patient { get; init; }
        public DateTime dateTime { get; init; }
        public string status { get; set; } //can use enums but idk how to
        public decimal feesCharged { get; set; }


        public Appointment(Doctor doc, Patient p, string status, decimal feesCharged)
        {
            this.doctor = doc;
            this.patient = p;
            this.status = status;
            this.feesCharged = feesCharged;
            this.dateTime = DateTime.Now;
        }

        public void cancelAppointment()
        {
            if(status=="Cancelled" || status == "Completed")
                {
                    throw new InvalidOperationException("Appointment is already " + status);
                }

                this.status = "Cancelled";
        }

        public void completeAppointment()
            {
                if (status == "Cancelled" || status == "Completed")
                {
                    throw new InvalidOperationException("Appointment is already " + status);
                }

                this.status = "Completed";
            }

    }

        public delegate void AppointmentHandler(string msg);
        class Clinic
        {
            public List<Doctor> doctors;
            public List<Patient> patients;
            List<Appointment> appointments;
            public event AppointmentHandler handler;

            public Clinic()
            {
                this.doctors = new List<Doctor>();
                patients = new List<Patient>();
                appointments = new List<Appointment>();
            }

            public void AddAppointment(Appointment appointment)
            {
                bool contains = appointments.Any(a => a.patient.name == appointment.patient.name && a.doctor.Name == appointment.doctor.Name && a.dateTime == appointment.dateTime && a.id != appointment.id);


                if (contains) throw new InvalidOperationException("Invalid appointment - same id or 2 appointments in same day");


                appointments.Add(appointment);
                handler?.Invoke("Appointment Booked");
            }

            public void CancelAppointment(int id)
            {
                Appointment appointment = appointments.FirstOrDefault(a => a.id == id);

                if (appointment == null) throw new InvalidOperationException("No appointemnt with thatd id");

                appointment.cancelAppointment();
                handler?.Invoke("Appointment cancelled");
            }

            public decimal GetTotalRevenue()
            {
                return appointments.Sum(a => a.feesCharged);
            }

            public int GetAppointmentsPerDoctor(string name)
            {
                return appointments.Count(a => a.doctor.Name.Equals(name));
            }

            public List<Patient> GetFreqPAtients()
            {
                return (List<Patient>)appointments.GroupBy(a => a.patient.name).Where(p => p.Count() > 1).Select(p => p.Key);
            }

            public Doctor GetBusiestDoctor()
            {
                int n = appointments.GroupBy(a => a.doctor.Name).Max(p => p.Count());
                return (Doctor)appointments.GroupBy(a => a.doctor.Name).Where(p => p.Count() == n).Select(p => p.Key);
            }

            public List<Appointment> filterByStatus(string status)
            {
                return appointments.Where(a => a.status.Equals(status)).ToList();
                //for date maybe just > < daterange
            }




        }

        }
    internal class Program
    {
        static void Main(string[] args)
        {
            //defining everything and delegates methods too
            
        }
    }
}
