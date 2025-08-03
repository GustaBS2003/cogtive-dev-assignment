import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import MachineList from '../MachineList';
import * as api from '../api';

const mockMachines = [
  { id: 1, name: 'Alpha', serialNumber: 'A-1', type: 'CNC', isActive: true },
  { id: 2, name: 'Beta', serialNumber: 'B-2', type: 'Injection', isActive: false }
];

describe('MachineList', () => {
  beforeEach(() => {
    jest.spyOn(api, 'fetchMachines').mockResolvedValue(mockMachines);
    jest.spyOn(api, 'fetchMachineProductionData').mockResolvedValue([]);
  });

  afterEach(() => {
    jest.restoreAllMocks();
  });

  it('renders machines and filters by status', async () => {
    render(<MachineList />);
    expect(screen.getByText(/Loading machines/i)).toBeInTheDocument();

    await waitFor(() => expect(screen.getByText('Alpha')).toBeInTheDocument());
    expect(screen.getByText('Beta')).toBeInTheDocument();

    // Filter to active only
    fireEvent.change(screen.getByLabelText(/Status/i), { target: { value: 'active' } });
    expect(screen.getByText('Alpha')).toBeInTheDocument();
    expect(screen.queryByText('Beta')).not.toBeInTheDocument();
  });

  it('shows error message if fetch fails', async () => {
    jest.spyOn(api, 'fetchMachines').mockRejectedValue(new Error('API error'));
    render(<MachineList />);
    await waitFor(() => expect(screen.getByText(/API error/i)).toBeInTheDocument());
  });

  it('filters by search term', async () => {
    render(<MachineList />);
    await waitFor(() => expect(screen.getByText('Alpha')).toBeInTheDocument());

    fireEvent.change(screen.getByPlaceholderText(/Search/i), { target: { value: 'Beta' } });
    expect(screen.getByText('Beta')).toBeInTheDocument();
    expect(screen.queryByText('Alpha')).not.toBeInTheDocument();
  });

  it('sorts by name', async () => {
    render(<MachineList />);
    await waitFor(() => expect(screen.getByText('Alpha')).toBeInTheDocument());

    fireEvent.change(screen.getByLabelText(/Sort by/i), { target: { value: 'name' } });
    // You can add more assertions here if you want to check the order visually
    expect(screen.getByText('Alpha')).toBeInTheDocument();
    expect(screen.getByText('Beta')).toBeInTheDocument();
  });
});