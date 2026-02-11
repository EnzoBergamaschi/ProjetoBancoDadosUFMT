import React, { useState, useEffect } from 'react';
import axios from 'axios';

const GenericCrud = ({ title, endpoint, fields, primaryKey = 'id' }) => {
    const [data, setData] = useState([]);
    const [form, setForm] = useState({});
    const [isEditing, setIsEditing] = useState(false);
    const [showModal, setShowModal] = useState(false);
    const [searchTerm, setSearchTerm] = useState('');

    useEffect(() => {
        fetchData();
    }, [endpoint]);

    const fetchData = async () => {
        try {
            const response = await axios.get(`http://localhost:8080/api/${endpoint}`);
            setData(response.data);
            setSearchTerm('');
        } catch (error) {
            console.error(`Error fetching ${endpoint}`, error);
        }
    };

    const handleSearch = async (e) => {
        e.preventDefault();
        if (!searchTerm) {
            fetchData();
            return;
        }

        try {
            // Only works for single-key endpoints (GET /api/medicos/1)
            // For Consultas (composite), we keep simple filtering or show not found
            const response = await axios.get(`http://localhost:8080/api/${endpoint}/${searchTerm}`);
            setData(response.data ? [response.data] : []);
        } catch (error) {
            console.error(`Error searching ${endpoint}`, error);
            setData([]);
            alert("Registro não encontrado ou tabela não suporta busca direta por ID único.");
        }
    };

    const handleOpenModal = (item = null) => {
        if (item) {
            setForm(item);
            setIsEditing(true);
        } else {
            setForm({});
            setIsEditing(false);
        }
        setShowModal(true);
    };

    const handleCloseModal = () => {
        setShowModal(false);
        setForm({});
        setIsEditing(false);
    };

    const handleInputChange = (e) => {
        const { name, value } = e.target;
        setForm({ ...form, [name]: value });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            if (isEditing) {
                const pkValue = Array.isArray(primaryKey) 
                    ? primaryKey.map(k => form[k]).join('/') 
                    : form[primaryKey];
                
                await axios.put(`http://localhost:8080/api/${endpoint}/${pkValue}`, form);
            } else {
                await axios.post(`http://localhost:8080/api/${endpoint}`, form);
            }
            handleCloseModal();
            fetchData();
        } catch (error) {
            console.error(`Error saving ${endpoint}`, error);
            alert("Erro ao salvar dados. Verifique o console.");
        }
    };

    const handleDelete = async (item) => {
        if (!window.confirm("Deseja realmente excluir?")) return;
        try {
            const pkValue = Array.isArray(primaryKey) 
                ? primaryKey.map(k => item[k]).join('/') 
                : item[primaryKey];
            
            const url = Array.isArray(primaryKey) 
                ? `http://localhost:8080/api/${endpoint}?${primaryKey.map(k => `${k}=${item[k]}`).join('&')}`
                : `http://localhost:8080/api/${endpoint}/${pkValue}`;

            await axios.delete(url);
            fetchData();
        } catch (error) {
            console.error(`Error deleting ${endpoint}`, error);
        }
    };

    return (
        <div className="crud-container">
            <div className="crud-header">
                <div className="header-left">
                    <h2>{title}</h2>
                    <form onSubmit={handleSearch} className="search-form">
                        <input 
                            type="text" 
                            placeholder="Buscar por ID..." 
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                        />
                        <button type="submit" className="btn-search">Buscar</button>
                        {data.length === 1 && <button type="button" onClick={fetchData} className="btn-clear">Limpar</button>}
                    </form>
                </div>
                <button className="btn-add" onClick={() => handleOpenModal()}>+ Novo Cadastro</button>
            </div>

            {showModal && (
                <div className="modal-overlay">
                    <div className="modal-content">
                        <h3>{isEditing ? `Editar ${title.slice(0, -1)}` : `Novo ${title.slice(0, -1)}`}</h3>
                        <form onSubmit={handleSubmit} className="crud-form">
                            {fields.map(field => (
                                <div key={field.name} className="form-group">
                                    <label>{field.label}</label>
                                    <input
                                        type={field.type || "text"}
                                        name={field.name}
                                        value={form[field.name] || ''}
                                        onChange={handleInputChange}
                                        required={field.required}
                                        disabled={isEditing && field.isPK}
                                    />
                                </div>
                            ))}
                            <div className="modal-actions">
                                <button type="submit" className="btn-save">{isEditing ? 'Atualizar' : 'Salvar'}</button>
                                <button type="button" className="btn-cancel" onClick={handleCloseModal}>Cancelar</button>
                            </div>
                        </form>
                    </div>
                </div>
            )}

            <div className="table-responsive">
                <table className="crud-table">
                    <thead>
                        <tr>
                            {fields.map(field => <th key={field.name}>{field.label}</th>)}
                            <th>Ações</th>
                        </tr>
                    </thead>
                    <tbody>
                        {data.map((item, index) => (
                            <tr key={index}>
                                {fields.map(field => <td key={field.name}>{String(item[field.name])}</td>)}
                                <td>
                                    <button className="btn-edit" onClick={() => handleOpenModal(item)}>Editar</button>
                                    <button className="btn-delete" onClick={() => handleDelete(item)}>Excluir</button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
};

export default GenericCrud;
