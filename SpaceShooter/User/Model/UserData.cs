using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace SpaceShooter.User
{
    public class UserData : INotifyPropertyChanged
    {
        private string _username;
        public string Name
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged(nameof(Name));
                    ValidateUser(); 
                }
            }
        }

        public UserData(string username)
        {
            _username = username;
            ValidateUser();  
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private void ValidateUser()
        {
            if (string.IsNullOrEmpty(Name)) 
            {
                Name = PromptForUsername();
            }

            if (string.IsNullOrEmpty(Name))
            {
                Environment.Exit(0);  
            }
        }

        private string PromptForUsername()
        {
            Cursor.Show();

            UsernameForm usernameForm = new UsernameForm();
            if (usernameForm.ShowDialog() == DialogResult.OK)
            {
                return usernameForm.Username;
            }

            Cursor.Hide();
            return null;
        }
    }
}
