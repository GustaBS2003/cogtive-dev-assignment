import React, { useEffect, useState } from 'react';
import { fetchMachines, fetchMachineProductionData } from './api';
import { Machine, ProductionData } from './models';
import './MachineList.css';

const MachineList: React.FC = () => {
    const [machines, setMachines] = useState<Machine[]>([]);
    const [selectedMachine, setSelectedMachine] = useState<number | null>(null);
    const [productionData, setProductionData] = useState<ProductionData[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        setLoading(true);
        fetchMachines()
            .then(data => {
                setMachines(data);
                setLoading(false);
            })
            .catch((err) => {
                console.error(err);
                setError('Failed to load machines');
                setLoading(false);
            });
    }, []);

    const handleMachineSelect = (machineId: number) => {
        setSelectedMachine(machineId);
        setLoading(true);
        fetchMachineProductionData(machineId)
            .then(data => {
                setProductionData(data);
                setLoading(false);
            })
            .catch((err) => {
                console.error(err);
                setError(`Failed to load production data for Machine #${machineId}`);
                setLoading(false);
            });
    };

    if (loading) {
        return (
            <div className="loading-spinner">
                <div className="spinner"></div>
                <div>Loading...</div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="error-container">
                <div className="error-message">{error}</div>
                <button onClick={() => setError(null)}>Dismiss</button>
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

            {selectedMachine && productionData.length > 0 && (
                <div className="production-data-container">
                    <h3>
                        Production Data for {machines.find(m => m.id === selectedMachine)?.name}
                    </h3>
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
                </div>
            )}

            {selectedMachine && productionData.length === 0 && (
                <div className="no-data-message">
                    No production data available for this machine.
                </div>
            )}
        </div>
    );
};

export default MachineList;
export { };