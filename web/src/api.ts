import axios from 'axios';
import { Machine, ProductionData } from './models';

const API_BASE = process.env.REACT_APP_API_BASE || 'http://localhost:5211';

export const fetchMachines = async (): Promise<Machine[]> => {
    try {
        const response = await axios.get<Machine[]>(`${API_BASE}/api/machines`);
        return response.data;
    }
    catch (err: any) {
        if (err.response) {
            throw new Error(`Server error: ${err.response.statusText}`);
        } else if (err.request) {
            throw new Error('Network error: Could not reach the server.');
        } else {
            throw new Error('Unexpected error occurred.');
        }
    }
};

export const fetchMachineById = async (id: number): Promise<Machine> => {
    try {
        const response = await axios.get<Machine>(`${API_BASE}/api/machines/${id}`);
        return response.data;
    }
    catch (err: any) {
        if (err.response) {
            throw new Error(`Server error: ${err.response.statusText}`);
        } else if (err.request) {
            throw new Error('Network error: Could not reach the server.');
        } else {
            throw new Error('Unexpected error occurred.');
        }
    }
};

export const fetchProductionData = async (): Promise<ProductionData[]> => {
    try {
        const response = await axios.get<ProductionData[]>(`${API_BASE}/api/production-data`);
        return response.data;
    }
    catch (err: any) {
        if (err.response) {
            throw new Error(`Server error: ${err.response.statusText}`);
        } else if (err.request) {
            throw new Error('Network error: Could not reach the server.');
        } else {
            throw new Error('Unexpected error occurred.');
        }
    }
};

export const fetchMachineProductionData = async (machineId: number): Promise<ProductionData[]> => {
    try {
        const response = await axios.get<ProductionData[]>(`${API_BASE}/api/machines/${machineId}/production-data`);
        return response.data;
    }
    catch (err: any) {
        if (err.response) {
            throw new Error(`Server error: ${err.response.statusText}`);
        } else if (err.request) {
            throw new Error('Network error: Could not reach the server.');
        } else {
            throw new Error('Unexpected error occurred.');
        }
    }
};

export const postProductionData = async (data: Omit<ProductionData, 'id'>): Promise<ProductionData> => {
    try {
        const response = await axios.post<ProductionData>(`${API_BASE}/api/production-data`, data);
        return response.data;
    }
    catch (err: any) {
        if (err.response) {
            throw new Error(`Server error: ${err.response.statusText}`);
        } else if (err.request) {
            throw new Error('Network error: Could not reach the server.');
        } else {
            throw new Error('Unexpected error occurred.');
        }
    }
};