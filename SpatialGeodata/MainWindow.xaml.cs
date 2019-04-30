﻿using Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Program
{
    public partial class MainWindow : Window
    {
        private Database database;
        private Polygon currentPolygon;
        private ObservableCollection<Building> observableCollectionBuildings = new ObservableCollection<Building>();
        private ObservableCollection<Point> observableCollectionPoints = new ObservableCollection<Point>();
        private ObservableCollection<Visitor> observableCollectionVisitors = new ObservableCollection<Visitor>();
        private List<Point> pointsCurrentPolygon = new List<Point>();
        private double sizeFactor = 1;
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                database = Database.GetInstance();
                listViewBuildings.ItemsSource = observableCollectionBuildings;
                dataGridCoordinates.ItemsSource = observableCollectionPoints;
                dataGridVisitors.ItemsSource = observableCollectionVisitors;
                FillBuildings();
                FillVisitors();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void FillVisitors()
        {
            observableCollectionVisitors.Clear();
            foreach (Visitor visitor in database.GetVisitors())
                observableCollectionVisitors.Add(visitor);
        }

        private void DrawVisitors()
        {
            canvas.Children.RemoveRange(1, observableCollectionVisitors.Count);
            foreach (Visitor v in observableCollectionVisitors)
            {
                Ellipse blueRectangle = new Ellipse();
                blueRectangle.Height = 10;
                blueRectangle.Width = 10;
                SolidColorBrush blueBrush = new SolidColorBrush(Colors.Blue);
                SolidColorBrush blackBrush = new SolidColorBrush(Colors.Black);
                blueRectangle.StrokeThickness = 4;
                blueRectangle.Stroke = blackBrush;
                blueRectangle.Fill = blueBrush;
                sizeFactor = (canvas.ActualHeight < canvas.ActualWidth ? canvas.ActualHeight : canvas.ActualWidth) / 1000.0;
                blueRectangle.SetValue(Canvas.LeftProperty, v.Position.X * sizeFactor);
                blueRectangle.SetValue(Canvas.TopProperty, v.Position.Y * sizeFactor);
                canvas.Children.Add(blueRectangle);
            }
        }

        private void DrawSelectedBuilding()
        {
            if (currentPolygon != null)
                canvas.Children.Remove(currentPolygon);
            currentPolygon = new Polygon();
            currentPolygon.Stroke = Brushes.Black;
            currentPolygon.Fill = Brushes.LightBlue;
            currentPolygon.StrokeThickness = 1;
            currentPolygon.HorizontalAlignment = HorizontalAlignment.Left;
            currentPolygon.VerticalAlignment = VerticalAlignment.Center;
            currentPolygon.Points = new PointCollection(pointsCurrentPolygon);
            canvas.Children.Add(currentPolygon);
        }

        /// <summary>
        /// Fills observable points list with Drawing.Points from DB by converting them to System.Windows.Points
        /// </summary>
        /// <param name="list">Drawing.Points list from DB</param>
        private void UpdateControlsToSelectedBuilding(List<System.Drawing.Point> list)
        {
            observableCollectionPoints.Clear();
            pointsCurrentPolygon.Clear();
            canvas.Children.Clear();
            sizeFactor = (canvas.ActualHeight < canvas.ActualWidth ? canvas.ActualHeight : canvas.ActualWidth) / 1000.0;
            list.ForEach(point => { observableCollectionPoints.Add(new Point(point.X, point.Y)); pointsCurrentPolygon.Add(new Point(point.X * sizeFactor, point.Y * sizeFactor)); });
            DrawSelectedBuilding();
            DrawVisitors();
        }
        private void FillBuildings()
        {
            observableCollectionBuildings.Clear();
            foreach (Building building in database.GetBuildings())
                observableCollectionBuildings.Add(building);
        }

        private void ListBuildings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateControlsToSelectedBuilding(((Building)listViewBuildings.SelectedItem).GetCollPoints());
            IList<Visitor> list = db.ReadVisitorOfBuilding((Building)listViewBuildings.SelectedItem);
            string text = "";
            foreach (var visitor in list)
                text += visitor.Name + ", ";
            if (text != "")
                text = text.Substring(0, text.Length - 3);
            txtVisitors.Content = text;
        }

        /// <summary>
        /// Resize the polygon and canvas accordingly when window is resized
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Dashboard_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            cvMap.Width = (border.ActualWidth < border.ActualHeight ? border.ActualWidth : border.ActualHeight) - 20;
            cvMap.Height = (border.ActualHeight < border.ActualWidth ? border.ActualHeight : border.ActualWidth) - 20;
            if ((Building)listViewBuildings.SelectedItem != null)
                UpdateControlsToSelectedBuilding(((Building)listViewBuildings.SelectedItem).GetCollPoints());
        }

        private void CvMap_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = Mouse.GetPosition(cvMap);
            txtCurrentCoordinates.Text = "X: " + (int)(p.X / size_factor) + ", Y: " + (int)(p.Y / size_factor);
        }

        private void CvMap_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = Mouse.GetPosition(relativeTo: cvMap);
            Visitor newVisitor = new Visitor(txtNewVisitorName.Text, new System.Drawing.Point((int)(p.X / size_factor), (int)(p.Y / size_factor)));
            try
            {
                db.addVisitor(newVisitor);
                obsVisitors.Add(newVisitor);
                DrawVisitors();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DgVisitors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string result = db.ReadBuildingWhereVisitorOccurs((Visitor)dgVisitors.SelectedItem);
            MessageBox.Show(result == "" ? "Außerhalb von Gebäuden!" : result);
        }
    }
}