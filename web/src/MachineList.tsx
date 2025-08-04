import React, { useEffect, useState } from 'react';
import { fetchMachines, fetchMachineProductionData } from './api';
import { Machine, ProductionData } from './models';
import './MachineList.css';
import * as signalR from '@microsoft/signalr';
import { Line } from 'react-chartjs-2';
import { Chart, LineElement, PointElement, LinearScale, CategoryScale, Tooltip, Legend } from 'chart.js';

Chart.register(LineElement, PointElement, LinearScale, CategoryScale, Tooltip, Legend);

type SortField = 'name' | 'type' | 'status';
type SortOrder = 'asc' | 'desc';
type StatusFilter = 'all' | 'active' | 'inactive';

const MachineList: React.FC = () => {
    const [machines, setMachines] = useState<Machine[]>([]);
    const [selectedMachine, setSelectedMachine] = useState<number | null>(null);
    const [productionData, setProductionData] = useState<ProductionData[]>([]);
    const [loadingMachines, setLoadingMachines] = useState<boolean>(true);
    const [loadingProduction, setLoadingProduction] = useState<boolean>(false);
    const [errorMachines, setErrorMachines] = useState<string | null>(null);
    const [errorProduction, setErrorProduction] = useState<string | null>(null);
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);

    // Enhancement state
    const [statusFilter, setStatusFilter] = useState<StatusFilter>('all');
    const [sortField, setSortField] = useState<SortField>('name');
    const [sortOrder, setSortOrder] = useState<SortOrder>('asc');
    const [searchTerm, setSearchTerm] = useState<string>('');

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

    // SignalR: conectar ao hub e ouvir novos dados
    useEffect(() => {
        const conn = new signalR.HubConnectionBuilder()
            .withUrl('http://localhost:5211/hubs/production')
            .withAutomaticReconnect()
            .build();

        let stopped = false;

        conn.start().then(() => {
            setConnection(conn);
            conn.on('ProductionDataAdded', (data: ProductionData) => {
                if (selectedMachine && data.machineId === selectedMachine) {
                    setProductionData(prev => [data, ...prev]);
                }
            });
        });

        return () => {
            stopped = true;
            conn.stop().catch(() => {});
        };
        // eslint-disable-next-line
    }, [selectedMachine]);

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

    // Filtering
    const filteredMachines = machines.filter(machine => {
        if (statusFilter === 'active' && !machine.isActive) return false;
        if (statusFilter === 'inactive' && machine.isActive) return false;
        if (searchTerm.trim() !== '') {
            const term = searchTerm.trim().toLowerCase();
            return (
                machine.name.toLowerCase().includes(term) ||
                machine.serialNumber.toLowerCase().includes(term)
            );
        }
        return true;
    });

    // Sorting
    const sortedMachines = [...filteredMachines].sort((a, b) => {
        let compare = 0;
        if (sortField === 'name') {
            compare = a.name.localeCompare(b.name);
        } else if (sortField === 'type') {
            compare = a.type.localeCompare(b.type);
        } else if (sortField === 'status') {
            compare = (a.isActive === b.isActive) ? 0 : a.isActive ? -1 : 1;
        }
        return sortOrder === 'asc' ? compare : -compare;
    });

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
            {/* --- Enhancements: Filter, Sort, Search --- */}
            <div className="machine-controls">
                <div>
                    <label>Status:&nbsp;
                        <select value={statusFilter} onChange={e => setStatusFilter(e.target.value as StatusFilter)}>
                            <option value="all">All</option>
                            <option value="active">Active</option>
                            <option value="inactive">Inactive</option>
                        </select>
                    </label>
                </div>
                <div>
                    <label>Sort by:&nbsp;
                        <select value={sortField} onChange={e => setSortField(e.target.value as SortField)}>
                            <option value="name">Name</option>
                            <option value="type">Type</option>
                            <option value="status">Status</option>
                        </select>
                        <button
                            className="sort-order-btn"
                            onClick={() => setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc')}
                            title={`Sort ${sortOrder === 'asc' ? 'descending' : 'ascending'}`}
                        >
                            {sortOrder === 'asc' ? '▲' : '▼'}
                        </button>
                    </label>
                </div>
                <div>
                    <input
                        type="text"
                        placeholder="Search by name or serial..."
                        value={searchTerm}
                        onChange={e => setSearchTerm(e.target.value)}
                        className="machine-search"
                    />
                </div>
            </div>
            {/* --- End Enhancements --- */}

            <div className="machine-cards">
                {sortedMachines.length === 0 ? (
                    <div className="no-data-message">No machines found.</div>
                ) : (
                    sortedMachines.map((machine) => (
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
                    ))
                )}
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
                        <>
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
                            {productionData.length > 0 && (
                                <div style={{ maxWidth: 700, margin: '0 auto 2rem auto' }}>
                                    <Line
                                        data={{
                                            labels: productionData
                                                .slice()
                                                .reverse()
                                                .map(d => new Date(d.timestamp).toLocaleTimeString()),
                                            datasets: [
                                                {
                                                    label: 'Efficiency (%)',
                                                    data: productionData
                                                        .slice()
                                                        .reverse()
                                                        .map(d => Number(d.efficiency)),
                                                    fill: false,
                                                    borderColor: 'rgb(75, 192, 192)',
                                                    tension: 0.2,
                                                },
                                            ],
                                        }}
                                        options={{
                                            responsive: true,
                                            plugins: {
                                                legend: { display: true },
                                                tooltip: { enabled: true },
                                            },
                                            scales: {
                                                y: { min: 0, max: 100, title: { display: true, text: 'Efficiency (%)' } },
                                                x: { title: { display: true, text: 'Time' } },
                                            },
                                        }}
                                    />
                                </div>
                            )}
                        </>
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