
using Microsoft.AspNetCore.SignalR.Client;

namespace Frontend.MAUI;
public partial class ClientPage : ContentPage
{
    private HubConnection connection;

    public ClientPage()
    {
        InitializeComponent();

        connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:9000/communication")
            .Build();

        InitializeSignalRConnection();
    }

    private async void InitializeSignalRConnection()
    {
        connection.On<string>("UpdateClient", (status) =>
        {
            Dispatcher.Dispatch(() =>
            {
                orderStatusLabel.Text = "Current Status: " + status;
            });
        });

        try
        {
            await connection.StartAsync();
            // Connection to the hub is established
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error starting SignalR connection: " + ex.Message);
        }
    }

    private async void SubscribeButton_Clicked(object sender, EventArgs e)
    {
        if (int.TryParse(poIDEntry.Text, out int poID))
        {
            try
            {
                await connection.InvokeAsync("SubscribeClient", poID);
                Console.WriteLine("Subscribed to PO ID: " + poID);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error invoking SubscribeClient: " + ex.Message);
            }
        }
        else
        {
            Console.WriteLine("Invalid PO ID.");
        }
    }
}
