import React, { useEffect, useState } from 'react';
import { fetchMachines, fetchMachineProductionData } from './api';
import { Machine, ProductionData } from './models';
import './MachineList.css';

const MachineList: React.FC = () => {
    const [machines, setMachines] = useState<Machine[]>([]);
    const [selectedMachine, setSelectedMachine] = useState<number | null>(null);
    const [productionData, setProductionData] = useState<ProductionData[]>([]);
    const [loadingMachines, setLoadingMachines] = useState<boolean>(true);
    const [loadingProduction, setLoadingProduction] = useState<boolean>(false);
    const [errorMachines, setErrorMachines] = useState<string | null>(null);
    const [errorProduction, setErrorProduction] = useState<string | null>(null);

    // Fetch machines on mount
    const loadMachines = () => {
        setLoadingMachines(true);
        setErrorMachines(null);
        fetchMachines()
            .then(data => {
                setMachines(data);
                setLoadingMachines(false);
            })
            .catch((err) => {
                setErrorMachines(err.message || 'Failed to load machines');
                setLoadingMachines(false);
            });
    };

    useEffect(() => {
        loadMachines();
    }, []);

    // Fetch production data for selected machine
    const handleMachineSelect = (machineId: number) => {
        setSelectedMachine(machineId);
        setProductionData([]);
        setErrorProduction(null);
        setLoadingProduction(true);
        fetchMachineProductionData(machineId)
            .then(data => {
                setProductionData(data);
                setLoadingProduction(false);
            })
            .catch((err) => {
                setErrorProduction(err.message || `Failed to load production data for Machine #${machineId}`);
                setLoadingProduction(false);
            });
    };

    // Loading state for machines
    if (loadingMachines) {
        return (
            <div className="loading-spinner">
                <div className="spinner"></div>
                <div>Loading machines...</div>
            </div>
        );
    }

    // Error state for machines
    if (errorMachines) {
        return (
            <div className="error-container">
                <div className="error-message">{errorMachines}</div>
                <button onClick={loadMachines}>Retry</button>
            </div>
        );
    }

    return (
        <div className="machines-container">
            <h2>Industrial Machines</h2>
            <div className="machine-cards">
                {machines.map((machine) => (
                    <div className="machine-card" key={machine.id}>
                        <div className="machine-header">
                            <span className="machine-name">{machine.name}</span>
                            <span className={`status-badge ${machine.isActive ? 'active' : 'inactive'}`}>
                                {machine.isActive ? 'Active' : 'Inactive'}
                            </span>
                        </div>
                        <div className="machine-details">
                            <div><strong>Serial:</strong> {machine.serialNumber}</div>
                            <div><strong>Type:</strong> {machine.type}</div>
                        </div>
                        <div className="machine-actions">
                            <button onClick={() => handleMachineSelect(machine.id)}>
                                View Production Data
                            </button>
                        </div>
                    </div>
                ))}
            </div>

            {selectedMachine && (
                <div className="production-data-container">
                    <h3>
                        Production Data for {machines.find(m => m.id === selectedMachine)?.name}
                    </h3>
                    {loadingProduction ? (
                        <div className="loading-spinner">
                            <div className="spinner"></div>
                            <div>Loading production data...</div>
                        </div>
                    ) : errorProduction ? (
                        <div className="error-container">
                            <div className="error-message">{errorProduction}</div>
                            <button onClick={() => handleMachineSelect(selectedMachine)}>Retry</button>
                        </div>
                    ) : productionData.length > 0 ? (
                        <table className="production-table" border={1} cellPadding={5}>
                            <thead>
                                <tr>
                                    <th>Timestamp</th>
                                    <th>Efficiency (%)</th>
                                    <th>Units Produced</th>
                                    <th>Downtime (min)</th>
                                </tr>
                            </thead>
                            <tbody>
                                {productionData.map((data) => (
                                    <tr key={data.id}>
                                        <td>{new Date(data.timestamp).toLocaleString()}</td>
                                        <td>{data.efficiency}</td>
                                        <td>{data.unitsProduced}</td>
                                        <td>{data.downtime}</td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    ) : (
                        <div className="no-data-message">
                            No production data available for this machine.
                        </div>
                    )}
                </div>
            )}
        </div>
    );
};

export default MachineList;
export {};