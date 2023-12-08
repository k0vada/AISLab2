using Client.Model;
using Client.Model.Controller;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static Client.Model.Controller.ClientController;

namespace Client.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Student> students;
        public ObservableCollection<Student> Students
        {
            get => students;
            set
            {
                students = value;
                OnPropertyChanged("Students");
            }
        }

        public Student SelectedStudent { get; set; }

        static IPAddress clientIP = IPAddress.Parse("127.0.0.1");
        const int clientPort = 8082;
        EndPoint udpEndPoint = new IPEndPoint(clientIP, clientPort);

        static IPAddress serverIP = IPAddress.Parse("127.0.0.1");
        const int serverPort = 8081;
        EndPoint serverEndPoint = new IPEndPoint(serverIP, serverPort);

        Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        private Command deleteStudent;
        public Command DeleteStudentCommand
        {
            get
            {
                return deleteStudent ?? (deleteStudent = new Command(obj =>
                {
                    DeleteStudent(udpSocket, serverEndPoint);
                }));
            }
        }
        private Command addStudent;

        public Command AddStudentCommand
        {
            get
            {
                return addStudent ?? (addStudent = new Command(obj =>
                {
                    AddStudent(udpSocket, serverEndPoint);
                }));
            }
        }

        public async void DeleteStudent(Socket udpSocket, EndPoint serverEndPoint)
        {
            if (!(SelectedStudent is null))
            {
                if (!(String.IsNullOrEmpty(SelectedStudent.FirstName) || String.IsNullOrEmpty(SelectedStudent.LastName)))
                {
                    SendData(udpSocket, serverEndPoint, "delete");
                    await SendDataAsync(udpSocket, serverEndPoint, SelectedStudent.Id.ToString());
                    SelectedStudent = Students.FirstOrDefault(s => s.Id == SelectedStudent.Id);
                    Thread.Sleep(2000);
                    Students.Remove(SelectedStudent);
                }
            }
        }
        public async void AddStudent(Socket udpSocket, EndPoint serverEndPoint)
        {
            if (!(SelectedStudent is null))
            {
                if (!(SelectedStudent.Id != 0 || String.IsNullOrEmpty(SelectedStudent.FirstName) || String.IsNullOrEmpty(SelectedStudent.LastName)))
                {
                    SendData(udpSocket, serverEndPoint, "add");
                    string _message = JsonSerializer.Serialize(SelectedStudent);
                    await SendDataAsync(udpSocket, serverEndPoint, _message);
                    Thread.Sleep(2000);
                    Students = GetStudents(udpSocket, serverEndPoint);
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public MainWindowViewModel()
        {
            udpSocket.Bind(udpEndPoint);
            Thread.Sleep(5000);
            Students = GetStudents(udpSocket, serverEndPoint);
        }


    }
}
