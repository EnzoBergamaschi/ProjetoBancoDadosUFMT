import React, { useEffect, useState } from 'react';
import axios from 'axios';

const ItemList = ({ onEdit, refresh }) => {
    const [items, setItems] = useState([]);

    useEffect(() => {
        fetchItems();
    }, [refresh]);

    const fetchItems = async () => {
        try {
            const response = await axios.get('http://localhost:8080/api/items');
            setItems(response.data);
        } catch (error) {
            console.error("Error fetching items", error);
        }
    };

    const handleDelete = async (id) => {
        if (window.confirm("Are you sure?")) {
            try {
                await axios.delete(`http://localhost:8080/api/items/${id}`);
                fetchItems();
            } catch (error) {
                console.error("Error deleting item", error);
            }
        }
    };

    return (
        <div>
            <h2>Items</h2>
            <table>
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Description</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    {items.map(item => (
                        <tr key={item.id}>
                            <td>{item.name}</td>
                            <td>{item.description}</td>
                            <td>
                                <button onClick={() => onEdit(item)}>Edit</button>
                                <button onClick={() => handleDelete(item.id)}>Delete</button>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
};

export default ItemList;
