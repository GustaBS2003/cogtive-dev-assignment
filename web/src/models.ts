export interface Machine {
  id: number;
  name: string;
  serialNumber: string;
  type: string;
  isActive: boolean;
}

export interface ProductionData {
  id: number;
  machineId: number;
  timestamp: string;
  // Intentional error: efficiency is a string instead of number
  efficiency: number;
  unitsProduced: number;
  downtime: number; // minutes
}