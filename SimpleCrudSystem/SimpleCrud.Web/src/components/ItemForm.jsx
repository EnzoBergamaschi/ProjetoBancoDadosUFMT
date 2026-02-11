import React, { useState, useEffect } from 'react';
import axios from 'axios';

const ItemForm = ({ itemToEdit, onSave }) => {
    const [name, setName] = useState('');
    const [description, setDescription] = useState('');

    useEffect(() => {
        if (itemToEdit) {
            setName(itemToEdit.name);
            setDescription(itemToEdit.description);
        } else {
            setName('');
            setDescription('');
        }
    }, [itemToEdit]);

    const handleSubmit = async (e) => {
        e.preventDefault();
        const item = { name, description };

        try {
            if (itemToEdit) {
                await axios.put(`http://localhost:8080/api/items/${itemToEdit.id}`, item);
            } else {
                await axios.post('http://localhost:8080/api/items', item);
            }
            onSave();
            setName('');
            setDescription('');
        } catch (error) {
            console.error("Error saving item", error);
        }
    };

    return (
        <form onSubmit={handleSubmit}>
            <h3>{itemToEdit ? 'Edit Item' : 'Add Item'}</h3>
            <div>
                <label>Name:</label>
                <input type="text" value={name} onChange={e => setName(e.target.value)} required />
            </div>
            <div>
                <label>Description:</label>
                <input type="text" value={description} onChange={e => setDescription(e.target.value)} />
            </div>
            <button type="submit">{itemToEdit ? 'Update' : 'Add'}</button>
            {itemToEdit && <button type="button" onClick={() => onSave()}>Cancel</button>}
        </form>
    );
};

export default ItemForm;
