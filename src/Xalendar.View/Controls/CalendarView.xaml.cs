using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xalendar.Api.Extensions;
using Xalendar.Api.Interfaces;
using Xalendar.Api.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XView = Xamarin.Forms.View;

namespace Xalendar.View.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CalendarView : ContentView
    {
        public static BindableProperty EventsProperty =
            BindableProperty.Create(
                nameof(Events),
                typeof(IList<ICalendarViewEvent>),
                typeof(CalendarView),
                null,
                BindingMode.OneWay,
                propertyChanged: OnEventsChanged);
        
        public IList<ICalendarViewEvent> Events
        {
            get => (IList<ICalendarViewEvent>)GetValue(EventsProperty);
            set => SetValue(EventsProperty, value);
        }
        
        private static void OnEventsChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            if (bindable is CalendarView calendarView && newvalue is IList<ICalendarViewEvent> events)
            {
                calendarView._monthContainer.AddEvents(events);
                calendarView.RecycleDays(calendarView._monthContainer.Days);
            }
        }
        
        private readonly MonthContainer _monthContainer;
        private readonly int _numberOfDaysInContainer;
        
        public CalendarView()
        {
            InitializeComponent();
            
            _monthContainer = new MonthContainer(DateTime.Today);

            var days = _monthContainer.Days;
            _numberOfDaysInContainer = days.Count;
            foreach (var _ in days)
                CalendarDaysContainer.Children.Add(new CalendarDay());
            RecycleDays(days);
            
            BindableLayout.SetItemsSource(CalendarDaysOfWeekContainer, _monthContainer.DaysOfWeek);
            MonthName.Text = _monthContainer.GetName();
        }

        private async void OnPreviousMonthClick(object sender, EventArgs e)
        {
            var result = await Task.Run(() =>
            {
                _monthContainer.Previous();
                
                var days = _monthContainer.Days;
                var monthName = _monthContainer.GetName();

                return (days, monthName);
            });

            MonthName.Text = result.monthName;
            RecycleDays(result.days);
        }

        private async void OnNextMonthClick(object sender, EventArgs e)
        {
            var result = await Task.Run(() =>
            {
                _monthContainer.Next();
                
                var days = _monthContainer.Days;
                var monthName = _monthContainer.GetName();

                return (days, monthName);
            });
            
            MonthName.Text = result.monthName;
            RecycleDays(result.days);
        }

        private void RecycleDays(IReadOnlyList<Day?> days)
        {
            for (var index = 0; index < _numberOfDaysInContainer; index++)
            {
                var day = days[index];
                var view = CalendarDaysContainer.Children[index];

                if (view.FindByName<XView>("HasEventsElement") is {} hasEventsElement)
                    hasEventsElement.IsVisible = day?.HasEvents ?? false;

                if (view.FindByName<XView>("DayContainer") is {} dayContainer)
                    dayContainer.BackgroundColor = day is {} && day.IsToday ? Color.Red : Color.Transparent;

                if (view.FindByName<Label>("DayElement") is {} dayElement)
                    dayElement.Text = day?.ToString();
            }
        }
    }
}

