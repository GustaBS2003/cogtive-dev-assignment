using Cogtive.App.Models;
using Cogtive.App.Services;
using CogtiveDevAssignment.Services;
using System.Collections.ObjectModel;

namespace Cogtive.App
{
    public partial class MainPage : ContentPage
    {
        private readonly ApiService _apiService;
        private readonly LocalStorageService _localStorageService;

        private ObservableCollection<Machine> _machines = new();
        private Machine _selectedMachine;

        public MainPage()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _localStorageService = new LocalStorageService();

            InitializeUi();
        }

        private async void InitializeUi()
        {
            await LoadMachinesAsync();
            await UpdateSyncStatusAsync();
        }

        private async Task LoadMachinesAsync()
        {
            try
            {
                SetLoading(true);
                ErrorMessage.IsVisible = false;

                if (_localStorageService.IsOnline())
                {
                    var machines = await _apiService.GetMachinesAsync();
                    _machines = new ObservableCollection<Machine>(machines);
                }
                else
                {
                    // Fallback para modo offline
                    _machines = new ObservableCollection<Machine>
                    {
                        new Machine { Id = 1, Name = "CNC Machine Alpha (Offline Cache)", SerialNumber = "CNC-2023-001", Type = "CNC", IsActive = true },
                        new Machine { Id = 2, Name = "Injection Molder Beta (Offline Cache)", SerialNumber = "INJ-2022-042", Type = "Injection", IsActive = true },
                    };
                    await DisplayAlert("Offline Mode", "You are currently working offline. Data will be synchronized when a connection is available.", "OK");
                }

                MachinePicker.ItemsSource = _machines.Select(m => m.Name).ToList();
                MachinePicker.SelectedIndex = -1;
                MachineStatusFrame.IsVisible = false;
                DataEntryFrame.IsVisible = false;
                ProductionDataList.ItemsSource = null;
            }
            catch (Exception ex)
            {
                ShowError($"Error loading machines: {ex.Message}");
            }
            finally
            {
                SetLoading(false);
            }
        }

        private async void OnMachineSelected(object sender, EventArgs e)
        {
            ErrorMessage.IsVisible = false;
            int selectedIndex = MachinePicker.SelectedIndex;
            if (selectedIndex == -1)
            {
                MachineStatusFrame.IsVisible = false;
                DataEntryFrame.IsVisible = false;
                ProductionDataList.ItemsSource = null;
                return;
            }

            _selectedMachine = _machines[selectedIndex];
            UpdateMachineStatusUi(_selectedMachine);

            // Limpa campos de entrada
            ClearForm();

            await LoadProductionDataAsync();
        }

        private void UpdateMachineStatusUi(Machine machine)
        {
            MachineName.Text = machine.Name;
            MachineSerial.Text = machine.SerialNumber;
            MachineType.Text = machine.Type;
            MachineStatus.Text = machine.IsActive ? "Active" : "Inactive";
            MachineStatusFrame.IsVisible = true;
            DataEntryFrame.IsVisible = machine.IsActive;
        }

        private async Task LoadProductionDataAsync()
        {
            if (_selectedMachine == null) return;

            try
            {
                SetLoading(true);
                ErrorMessage.IsVisible = false;

                List<ProductionData> productionData;
                if (_localStorageService.IsOnline())
                {
                    productionData = await _apiService.GetMachineProductionDataAsync(_selectedMachine.Id);
                }
                else
                {
                    // Exibe apenas dados pendentes do local storage para a máquina selecionada
                    var pending = await _localStorageService.GetPendingProductionDataAsync();
                    productionData = pending.Where(p => p.MachineId == _selectedMachine.Id).ToList();
                }

                ProductionDataList.ItemsSource = productionData;
            }
            catch (Exception ex)
            {
                ShowError($"Error loading production data: {ex.Message}");
            }
            finally
            {
                SetLoading(false);
            }
        }

        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            if (_selectedMachine == null) return;

            if (!ValidateInputs(out decimal efficiency, out int unitsProduced, out int downtime))
            {
                await DisplayAlert("Validation Error", "All fields are required and must be valid numbers.", "OK");
                return;
            }

            try
            {
                SetLoading(true);
                ErrorMessage.IsVisible = false;

                var productionData = new ProductionData
                {
                    MachineId = _selectedMachine.Id,
                    Timestamp = DateTime.UtcNow,
                    Efficiency = efficiency,
                    UnitsProduced = unitsProduced,
                    Downtime = downtime
                };

                if (_localStorageService.IsOnline())
                {
                    await _apiService.PostProductionDataAsync(productionData);
                    await DisplayAlert("Success", "Production data submitted successfully", "OK");
                }
                else
                {
                    await _localStorageService.SavePendingProductionDataAsync(productionData);
                    await DisplayAlert("Offline Mode", "Data saved for later synchronization", "OK");
                }

                ClearForm();
                await LoadProductionDataAsync();
                await UpdateSyncStatusAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to submit data: {ex.Message}", "OK");
            }
            finally
            {
                SetLoading(false);
            }
        }

        private bool ValidateInputs(out decimal efficiency, out int unitsProduced, out int downtime)
        {
            efficiency = 0;
            unitsProduced = 0;
            downtime = 0;
            return decimal.TryParse(EfficiencyEntry.Text, out efficiency)
                && int.TryParse(UnitsProducedEntry.Text, out unitsProduced)
                && int.TryParse(DowntimeEntry.Text, out downtime);
        }

        private void ClearForm()
        {
            EfficiencyEntry.Text = string.Empty;
            UnitsProducedEntry.Text = string.Empty;
            DowntimeEntry.Text = string.Empty;
        }

        private async Task UpdateSyncStatusAsync()
        {
            int pendingCount = await _localStorageService.GetPendingProductionDataCountAsync();
            PendingUploadsLabel.Text = pendingCount.ToString();
            SyncButton.IsEnabled = pendingCount > 0 && _localStorageService.IsOnline();
        }

        private async void OnSyncClicked(object sender, EventArgs e)
        {
            if (!_localStorageService.IsOnline())
            {
                await DisplayAlert("Offline", "Cannot sync while offline", "OK");
                return;
            }

            try
            {
                SetLoading(true);
                ErrorMessage.IsVisible = false;

                var pendingData = await _localStorageService.GetPendingProductionDataAsync();
                var failedData = new List<ProductionData>();
                int syncedCount = 0;

                foreach (var data in pendingData)
                {
                    try
                    {
                        await _apiService.PostProductionDataAsync(data);
                        syncedCount++;
                    }
                    catch
                    {
                        // Se falhar, mantém no local storage para próxima tentativa
                        failedData.Add(data);
                    }
                }

                // Atualiza o local storage apenas com os que falharam
                if (failedData.Count > 0)
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(failedData);
                    await SecureStorage.SetAsync("pending_production_data", json);
                }
                else
                {
                    await _localStorageService.ClearPendingProductionDataAsync();
                }

                await UpdateSyncStatusAsync();
                await DisplayAlert("Sync Complete", $"Successfully synchronized {syncedCount} records", "OK");
                await LoadProductionDataAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Sync Error", $"Failed to synchronize: {ex.Message}", "OK");
            }
            finally
            {
                SetLoading(false);
            }
        }

        private async void OnScanQRClicked(object sender, EventArgs e)
        {
            await DisplayAlert("QR Scanner", "This is a simulated QR code scanner. In a full implementation, this would open the device camera to scan equipment QR codes.", "OK");
        }

        // Helper methods
        private void SetLoading(bool isLoading)
        {
            LoadingIndicator.IsVisible = isLoading;
            LoadingIndicator.IsRunning = isLoading;
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.IsVisible = true;
        }
    }
}
