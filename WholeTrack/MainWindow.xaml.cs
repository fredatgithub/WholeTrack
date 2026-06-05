using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
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
    private double _basePxPerYear = 50.0;
    private double _scale = 1.0;

    public MainWindow()
    {
      InitializeComponent();

      DeathBCCheckBox.IsEnabled = false;
      BirthBCCheckBox.IsEnabled = true;

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
      DeathDatePicker.IsEnabled = DeathUnknownCheckBox.IsChecked != true;
      DeathBCCheckBox.IsEnabled = DeathUnknownCheckBox.IsChecked != true;
    }

    private void IsDeadCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
      DeathDatePicker.IsEnabled = false;
      DeathDatePicker.SelectedDate = null;
      DeathUnknownCheckBox.IsChecked = false;
      DeathBCCheckBox.IsChecked = false;
      DeathBCCheckBox.IsEnabled = false;
    }

    private void BirthUnknownCheckBox_Checked(object sender, RoutedEventArgs e)
    {
      BirthDatePicker.IsEnabled = false;
      BirthBCCheckBox.IsChecked = false;
      BirthBCCheckBox.IsEnabled = false;
    }

    private void BirthUnknownCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
      BirthDatePicker.IsEnabled = true;
      BirthBCCheckBox.IsEnabled = true;
    }

    private void DeathUnknownCheckBox_Checked(object sender, RoutedEventArgs e)
    {
      DeathDatePicker.IsEnabled = false;
      DeathDatePicker.SelectedDate = null;
      DeathBCCheckBox.IsChecked = false;
      DeathBCCheckBox.IsEnabled = false;
    }

    private void DeathUnknownCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
      DeathBCCheckBox.IsEnabled = true;
      DeathDatePicker.IsEnabled = IsDeadCheckBox.IsChecked == true;
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

      if (BirthUnknownCheckBox.IsChecked != true && !BirthDatePicker.SelectedDate.HasValue)
      {
        MessageBox.Show("Veuillez sélectionner une date de naissance ou cocher Inconnue.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }

      var birthDate = BirthUnknownCheckBox.IsChecked == true
        ? UniversalDateTime.Unknown
        : UniversalDateTime.FromDateTime(BirthDatePicker.SelectedDate.Value, BirthBCCheckBox.IsChecked == true);

      UniversalDateTime deathDate = UniversalDateTime.Unknown;
      if (IsDeadCheckBox.IsChecked == true)
      {
        if (DeathUnknownCheckBox.IsChecked == true)
        {
          deathDate = UniversalDateTime.Unknown;
        }
        else if (DeathDatePicker.SelectedDate.HasValue)
        {
          deathDate = UniversalDateTime.FromDateTime(DeathDatePicker.SelectedDate.Value, DeathBCCheckBox.IsChecked == true);
        }
      }

      var p = new Person
      {
        FirstName = first,
        LastName = last,
        BirthDate = birthDate,
        Occupation = OccupationTextBox.Text?.Trim(),
        IsDead = IsDeadCheckBox.IsChecked == true,
        DeathDate = deathDate
      };

      _persons.Add(p);
      SavePersons();
      RenderTimeline();

      // clear inputs
      FirstNameTextBox.Text = string.Empty;
      LastNameTextBox.Text = string.Empty;
      OccupationTextBox.Text = string.Empty;
      BirthDatePicker.SelectedDate = DateTime.Today;
      BirthUnknownCheckBox.IsChecked = false;
      BirthBCCheckBox.IsChecked = false;
      BirthBCCheckBox.IsEnabled = true;
      IsDeadCheckBox.IsChecked = false;
      DeathDatePicker.SelectedDate = null;
      DeathUnknownCheckBox.IsChecked = false;
      DeathBCCheckBox.IsChecked = false;
      DeathBCCheckBox.IsEnabled = false;
    }

    private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      _scale = e.NewValue;
      RenderTimeline();
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
              _persons = obj.Where(p => p != null).ToList();
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
      if (TimelineCanvas == null)
        return;

      TimelineCanvas.Children.Clear();

      var knownBirthPersons = _persons?.Where(p => p != null && p.BirthDate != null && !p.BirthDate.IsUnknown).ToList() ?? new List<Person>();
      var unknownBirthPersons = _persons?.Where(p => p != null && (p.BirthDate == null || p.BirthDate.IsUnknown)).ToList() ?? new List<Person>();

      if (knownBirthPersons.Count == 0 && unknownBirthPersons.Count == 0)
      {
        TimelineCanvas.Width = 800;
        TimelineCanvas.Height = 200;
        var placeholder = new TextBlock
        {
          Text = "Aucun personnage à afficher.",
          FontSize = 14,
          Foreground = Brushes.Gray
        };
        Canvas.SetLeft(placeholder, 20);
        Canvas.SetTop(placeholder, 20);
        TimelineCanvas.Children.Add(placeholder);
        return;
      }

      int minYear = DateTime.Now.Year - 100;
      int maxYear = DateTime.Now.Year + 10;

      if (knownBirthPersons.Count > 0)
      {
        minYear = knownBirthPersons.Min(p => p.BirthDate.SortYear);
        maxYear = knownBirthPersons.Max(p => p.IsDead && p.DeathDate != null && !p.DeathDate.IsUnknown
          ? p.DeathDate.SortYear
          : DateTime.Now.Year);
        minYear = Math.Min(minYear, DateTime.Now.Year - 100);
        maxYear = Math.Max(maxYear, DateTime.Now.Year + 10);
      }

      double pxPerYear = _basePxPerYear * _scale;
      double width = (maxYear - minYear + 1) * pxPerYear + 100;
      if (width < 800)
        width = 800;

      TimelineCanvas.Width = width;
      double baselineY = 60;
      double unknownSectionHeight = 0;
      double requiredHeight = 200;
      TimelineCanvas.Height = requiredHeight;

      if (unknownBirthPersons.Count > 0)
      {
        unknownSectionHeight = 30 + unknownBirthPersons.Count * 20;
        baselineY += unknownSectionHeight;
        requiredHeight = Math.Max(requiredHeight, unknownSectionHeight + 60);

        var unknownHeader = new TextBlock
        {
          Text = "Dates de naissance inconnues :",
          FontSize = 12,
          FontWeight = FontWeights.Bold,
          Foreground = Brushes.DarkRed
        };
        Canvas.SetLeft(unknownHeader, 10);
        Canvas.SetTop(unknownHeader, 10);
        TimelineCanvas.Children.Add(unknownHeader);

        for (int i = 0; i < unknownBirthPersons.Count; i++)
        {
          var p = unknownBirthPersons[i];
          var text = $"{p.LastName} {p.FirstName}";
          if (!string.IsNullOrWhiteSpace(p.Occupation))
            text += $" — {p.Occupation}";

          var itemTb = new TextBlock
          {
            Text = text,
            FontSize = 11,
            Foreground = Brushes.Black
          };
          Canvas.SetLeft(itemTb, 10);
          Canvas.SetTop(itemTb, 30 + i * 20);
          TimelineCanvas.Children.Add(itemTb);
        }
      }

      if (knownBirthPersons.Count > 0)
      {
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
      }

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
      TimelineCanvas.Height = requiredHeight;

      for (int i = 0; i < knownBirthPersons.Count; i++)
      {
        var p = knownBirthPersons[i];
        double x = (p.BirthDate.SortYear - minYear) * pxPerYear + 50;
        double y = baselineY + 10 + i * 30;
        requiredHeight = Math.Max(requiredHeight, y + 60);

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

        double nextLineY = y + 16;

        if (!string.IsNullOrWhiteSpace(p.Occupation))
        {
          var occTb = new TextBlock
          {
            Text = p.Occupation,
            FontSize = 10,
            Foreground = Brushes.Gray
          };
          Canvas.SetLeft(occTb, x - 40);
          Canvas.SetTop(occTb, nextLineY);
          TimelineCanvas.Children.Add(occTb);
          nextLineY += 14;
        }

        var deathText = "Vivante";
        if (p.IsDead)
        {
          deathText = p.DeathDate == null || p.DeathDate.IsUnknown
            ? "Décès : Inconnue"
            : $"Décès : {p.DeathDate}";
        }

        var deathTb = new TextBlock
        {
          Text = deathText,
          FontSize = 10,
          Foreground = Brushes.DarkSlateGray
        };
        Canvas.SetLeft(deathTb, x - 40);
        Canvas.SetTop(deathTb, nextLineY);
        TimelineCanvas.Children.Add(deathTb);
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
