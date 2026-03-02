import React, { useState, useEffect } from 'react';
import axios from 'axios';

const GenericCrud = ({ title, endpoint, fields, primaryKey = 'id' }) => {
    const [data, setData] = useState([]);
    const [form, setForm] = useState({});
    const [isEditing, setIsEditing] = useState(false);
    const [showModal, setShowModal] = useState(false);
    const [searchTerm, setSearchTerm] = useState('');
    const [simulateRollback, setSimulateRollback] = useState(false);

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
            const rollbackParam = simulateRollback ? '?rollback=true' : '';
            if (isEditing) {
                const pkValue = Array.isArray(primaryKey) 
                    ? primaryKey.map(k => form[k]).join('/') 
                    : form[primaryKey];
                
                await axios.put(`http://localhost:8080/api/${endpoint}/${pkValue}${rollbackParam}`, form);
            } else {
                await axios.post(`http://localhost:8080/api/${endpoint}${rollbackParam}`, form);
            }
            if (simulateRollback) {
               // Se passou sem erro (o que num Rollback Forçado não deve ocorrer), a gente avisa
               alert("Transação completada com sucesso.");
            } else {
               alert("✅ Transação salva no banco! (COMMIT efetuado)");
            }
            handleCloseModal();
            fetchData();
        } catch (error) {
            console.error(`Error saving ${endpoint}`, error);
            const msg = error.response?.data?.detail || "Erro ao salvar dados. Verifique o console.";
            alert("⚠️ FALHA NA TRANSAÇÃO \n\nOcorreu um erro e o Banco de Dados desfez a operação inteira através de um ROLLBACK.\n\nDetalhes: " + msg);
        }
    };

    const handleDelete = async (item) => {
        if (!window.confirm("Deseja listar a remoção neste evento transacional?")) return;
        try {
            const pkValue = Array.isArray(primaryKey) 
                ? primaryKey.map(k => item[k]).join('/') 
                : item[primaryKey];
            
            const rollbackFlag = simulateRollback ? 'rollback=true' : '';
            const url = Array.isArray(primaryKey) 
                ? `http://localhost:8080/api/${endpoint}?${primaryKey.map(k => `${k}=${item[k]}`).join('&')}${simulateRollback ? '&' + rollbackFlag : ''}`
                : `http://localhost:8080/api/${endpoint}/${pkValue}${simulateRollback ? '?' + rollbackFlag : ''}`;

            await axios.delete(url);
            if (!simulateRollback) {
                alert("✅ Remoção salva no banco! (COMMIT efetuado)");
            }
            fetchData();
        } catch (error) {
            console.error(`Error deleting ${endpoint}`, error);
            const msg = error.response?.data?.detail || "Erro ao excluir dados.";
            alert("⚠️ FALHA NA TRANSAÇÃO \n\nOcorreu um erro e o Banco de Dados desfez a remoção (ROLLBACK).\n\nDetalhes: " + msg);
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
                <div style={{ display: 'flex', alignItems: 'center', gap: '15px' }}>
                    <label style={{ display: 'flex', alignItems: 'center', gap: '5px', backgroundColor: '#ffeeba', color: '#856404', padding: '8px 12px', borderRadius: '4px', fontWeight: 'bold', cursor: 'pointer' }}>
                        <input 
                            type="checkbox" 
                            checked={simulateRollback} 
                            onChange={(e) => setSimulateRollback(e.target.checked)} 
                            style={{margin: 0, width: 'auto'}}
                        />
                        Forçar Erro p/ Ver Rollback
                    </label>
                    <button className="btn-add" onClick={() => handleOpenModal()}>+ Novo Cadastro</button>
                </div>
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
