"""Tests para funciones de similitud — puro NumPy, sin modelo ML."""

import numpy as np
import pytest
from app.core.similarity import cosine_similarity, cosine_similarity_batch, find_best_match


def test_identical_vectors_return_one():
    v = np.random.randn(512).astype(np.float32)
    assert cosine_similarity(v, v) == pytest.approx(1.0, abs=1e-5)


def test_orthogonal_vectors_return_zero():
    a = np.zeros(512, dtype=np.float32)
    b = np.zeros(512, dtype=np.float32)
    a[0] = 1.0
    b[1] = 1.0
    assert cosine_similarity(a, b) == pytest.approx(0.0, abs=1e-5)


def test_opposite_vectors_return_negative():
    v = np.random.randn(512).astype(np.float32)
    assert cosine_similarity(v, -v) == pytest.approx(-1.0, abs=1e-5)


def test_batch_matches_individual():
    query = np.random.randn(512).astype(np.float32)
    gallery = np.random.randn(10, 512).astype(np.float32)

    batch_results = cosine_similarity_batch(query, gallery)
    individual_results = [cosine_similarity(query, g) for g in gallery]

    np.testing.assert_allclose(batch_results, individual_results, atol=1e-5)


def test_find_best_match_returns_correct_index():
    query = np.random.randn(512).astype(np.float32)
    gallery = np.random.randn(5, 512).astype(np.float32)
    # Hacer que el índice 3 sea idéntico al query
    gallery[3] = query

    idx, sim = find_best_match(query, gallery, threshold=0.5)
    assert idx == 3
    assert sim == pytest.approx(1.0, abs=1e-4)


def test_find_best_match_below_threshold():
    query = np.array([1.0] + [0.0] * 511, dtype=np.float32)
    gallery = np.array([[0.0, 1.0] + [0.0] * 510], dtype=np.float32)

    idx, sim = find_best_match(query, gallery, threshold=0.5)
    assert idx == -1


def test_empty_gallery():
    query = np.random.randn(512).astype(np.float32)
    idx, sim = find_best_match(query, np.array([]), threshold=0.5)
    assert idx == -1
    assert sim == 0.0
