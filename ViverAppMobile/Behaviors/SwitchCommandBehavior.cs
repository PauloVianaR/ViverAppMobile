using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ViverAppMobile.Behaviors
{
    public class SwitchCommandBehavior : Behavior<Switch>
    {
        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(SwitchCommandBehavior));

        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(SwitchCommandBehavior));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        protected override void OnAttachedTo(Switch bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.Toggled += OnToggled;
        }

        protected override void OnDetachingFrom(Switch bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.Toggled -= OnToggled;
        }

        private void OnToggled(object? sender, ToggledEventArgs e)
        {
            if (Command?.CanExecute(CommandParameter) ?? false)
            {
                Command.Execute(new { IsToggled = e.Value, Parameter = CommandParameter });
            }
        }
    }

}
