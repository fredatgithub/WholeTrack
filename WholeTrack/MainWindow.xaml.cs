using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WholeTrack.Models;

namespace WholeTrack
{
  /// <summary>
  /// Logique d'interaction pour MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private List<Person> _persons = new List<Person>();
    private readonly string _dataFile;
    private readonly string _settingsFile;

    public MainWindow()
    {
      InitializeComponent();

      var appDir = AppDomain.CurrentDomain.BaseDirectory;
      _dataFile = Path.Combine(appDir, "persons.json");
      _settingsFile = Path.Combine(appDir, "windowsettings.json");

      LoadWindowSettings();
      LoadPersons();
      RenderTimeline();

      this.Closing += MainWindow_Closing;
    }

    private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      SaveWindowSettings();
      SavePersons();
    }

    private void IsDeadCheckBox_Checked(object sender, RoutedEventArgs e)
    {
      DeathDatePicker.IsEnabled = true;
    }

    private void IsDeadCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
      DeathDatePicker.IsEnabled = false;
      DeathDatePicker.SelectedDate = null;
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
      var last = LastNameTextBox.Text?.Trim() ?? string.Empty;
      var first = FirstNameTextBox.Text?.Trim() ?? string.Empty;
      if (string.IsNullOrEmpty(last) && string.IsNullOrEmpty(first))
      {
        MessageBox.Show("Veuillez entrer au moins un nom ou un prénom.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }

      if (!BirthDatePicker.SelectedDate.HasValue)
      {
        MessageBox.Show("Veuillez sélectionner une date de naissance.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }

      var p = new Person
      {
        FirstName = first,
        LastName = last,
        BirthDate = BirthDatePicker.SelectedDate.Value,
        IsDead = IsDeadCheckBox.IsChecked == true,
        DeathDate = IsDeadCheckBox.IsChecked == true ? DeathDatePicker.SelectedDate : null
      };

      _persons.Add(p);
      SavePersons();
      RenderTimeline();

      // clear inputs
      FirstNameTextBox.Text = string.Empty;
      LastNameTextBox.Text = string.Empty;
      BirthDatePicker.SelectedDate = DateTime.Today;
      IsDeadCheckBox.IsChecked = false;
      DeathDatePicker.SelectedDate = null;
    }

    private void LoadPersons()
    {
      try
      {
        if (File.Exists(_dataFile))
        {
          using (var fs = File.OpenRead(_dataFile))
          {
            var ser = new DataContractJsonSerializer(typeof(List<Person>));
            var obj = ser.ReadObject(fs) as List<Person>;
            if (obj != null)
              _persons = obj;
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Erreur en chargeant les personnages: " + ex.Message);
      }
    }

    private void SavePersons()
    {
      try
      {
        using (var fs = File.Create(_dataFile))
        {
          var ser = new DataContractJsonSerializer(typeof(List<Person>));
          ser.WriteObject(fs, _persons);
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Erreur en sauvegardant les personnages: " + ex.Message);
      }
    }

    private void RenderTimeline()
    {
      TimelineCanvas.Children.Clear();

      if (_persons == null || _persons.Count == 0)
      {
        TimelineCanvas.Width = 800;
        return;
      }

      int minYear = _persons.Min(p => p.BirthDate.Year);
      int maxYear = _persons.Max(p => p.IsDead && p.DeathDate.HasValue ? p.DeathDate.Value.Year : DateTime.Now.Year);
      // add some padding years
      minYear = Math.Min(minYear, DateTime.Now.Year - 100);
      maxYear = Math.Max(maxYear, DateTime.Now.Year + 10);

      const double pxPerYear = 50.0;
      double width = (maxYear - minYear + 1) * pxPerYear + 100;
      TimelineCanvas.Width = width;
      double baselineY = 60;

      // draw year ticks
      for (int y = minYear; y <= maxYear; y++)
      {
        double x = (y - minYear) * pxPerYear + 50;
        var line = new System.Windows.Shapes.Line
        {
          X1 = x,
          X2 = x,
          Y1 = baselineY - 6,
          Y2 = baselineY + 6,
          Stroke = Brushes.Gray,
          StrokeThickness = 1
        };
        TimelineCanvas.Children.Add(line);

        var tb = new TextBlock
        {
          Text = y.ToString(),
          Foreground = Brushes.Black,
          FontSize = 10
        };
        Canvas.SetLeft(tb, x - 12);
        Canvas.SetTop(tb, baselineY - 26);
        TimelineCanvas.Children.Add(tb);
      }

      // baseline
      var baseLine = new System.Windows.Shapes.Line
      {
        X1 = 0,
        X2 = width,
        Y1 = baselineY,
        Y2 = baselineY,
        Stroke = Brushes.Black,
        StrokeThickness = 1
      };
      TimelineCanvas.Children.Add(baseLine);

      // draw persons; stack vertically
      for (int i = 0; i < _persons.Count; i++)
      {
        var p = _persons[i];
        double x = (p.BirthDate.Year - minYear) * pxPerYear + 50;
        double y = baselineY + 10 + i * 22;

        // marker
        var ellipse = new System.Windows.Shapes.Ellipse
        {
          Width = 8,
          Height = 8,
          Fill = p.IsDead ? Brushes.DarkRed : Brushes.DarkBlue
        };
        Canvas.SetLeft(ellipse, x - 4);
        Canvas.SetTop(ellipse, baselineY - 4);
        TimelineCanvas.Children.Add(ellipse);

        var nameTb = new TextBlock
        {
          Text = $"{p.LastName} {p.FirstName}",
          FontSize = 12
        };
        Canvas.SetLeft(nameTb, x - 40);
        Canvas.SetTop(nameTb, y);
        TimelineCanvas.Children.Add(nameTb);
      }
    }

    private void SaveWindowSettings()
    {
      try
      {
        var data = new WindowSettings
        {
          Left = this.Left,
          Top = this.Top,
          Width = this.Width,
          Height = this.Height,
          WindowState = this.WindowState
        };
        var ser = new DataContractJsonSerializer(typeof(WindowSettings));
        using (var fs = File.Create(_settingsFile))
        {
          ser.WriteObject(fs, data);
        }
      }
      catch { }
    }

    private void LoadWindowSettings()
    {
      try
      {
        if (!File.Exists(_settingsFile))
          return;
        var ser = new DataContractJsonSerializer(typeof(WindowSettings));
        using (var fs = File.OpenRead(_settingsFile))
        {
          var data = ser.ReadObject(fs) as WindowSettings;
          if (data != null)
          {
            this.Left = data.Left;
            this.Top = data.Top;
            this.Width = data.Width;
            this.Height = data.Height;
            this.WindowState = data.WindowState;
          }
        }
      }
      catch { }
    }

    [DataContract]
    private class WindowSettings
    {
      [DataMember]
      public double Left { get; set; }
      [DataMember]
      public double Top { get; set; }
      [DataMember]
      public double Width { get; set; }
      [DataMember]
      public double Height { get; set; }
      [DataMember]
      public WindowState WindowState { get; set; }
    }
  }
}
