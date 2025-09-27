using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Utility helpers for batch operations on Atoms (Person atoms) to safely move/disable collisions/reset physics.
// Conservative, small API so we don't accidentally change game state.
public static class AtomBatch
{
    // Return all Person atoms in the scene
    public static List<Atom> GetAllPersonAtoms()
    {
        try
        {
            return SuperController.singleton.GetAtoms().Where(a => a != null && a.type == "Person").ToList();
        }
        catch (Exception ex)
        {
            SuperController.LogMessage("AtomBatch.GetAllPersonAtoms error: " + ex.Message);
            return new List<Atom>();
        }
    }

    // Try to find an unoccupied position near origin by raycasting or simple grid search.
    // Returns true if found with out pos.
    public static bool TryFindUnoccupiedPosition(out Vector3 pos, float radius = 0.5f, int maxAttempts = 50)
    {
        pos = Vector3.zero;
        for (int i = 0; i < maxAttempts; i++)
        {
            var candidate = new Vector3(UnityEngine.Random.Range(-2f, 2f), 0.5f, UnityEngine.Random.Range(-2f, 2f));
            // simple check: overlap sphere against physics layers except UI (ignore layer 8 which VaM uses for UI sometimes)
            Collider[] hits = Physics.OverlapSphere(candidate, radius, ~0);
            if (hits == null || hits.Length == 0)
            {
                pos = candidate;
                return true;
            }
        }
        return false;
    }

    // Move atom to an unoccupied position near origin; returns true if moved
    public static bool MoveToUnoccupiedPosition(Atom atom)
    {
        if (atom == null) return false;
        Vector3 pos;
        if (!TryFindUnoccupiedPosition(out pos))
        {
            SuperController.LogMessage("AtomBatch.MoveToUnoccupiedPosition: failed to find unoccupied position");
            return false;
        }

        try
        {
            atom.transform.position = pos;
            // also try resetting rotation
            atom.transform.rotation = Quaternion.identity;
            return true;
        }
        catch (Exception ex)
        {
            SuperController.LogMessage("AtomBatch.MoveToUnoccupiedPosition error: " + ex.Message);
            return false;
        }
    }

    // Disable collisions for an atom by setting collisionEnabled = false
    public static void DisableCollision(Atom atom)
    {
        if (atom == null) return;
        try
        {
            atom.collisionEnabled = false;
        }
        catch (Exception ex)
        {
            SuperController.LogMessage("AtomBatch.DisableCollision error: " + ex.Message);
        }
    }

    // Enable collisions for an atom
    public static void EnableCollision(Atom atom)
    {
        if (atom == null) return;
        try
        {
            atom.collisionEnabled = true;
        }
        catch (Exception ex)
        {
            SuperController.LogMessage("AtomBatch.EnableCollision error: " + ex.Message);
        }
    }

    // Reset physics on atom: try to reset all rigidbodies in atom
    public static void ResetPhysics(Atom atom)
    {
        if (atom == null) return;
        try
        {
            var rbs = atom.GetComponentsInChildren<Rigidbody>(true);
            foreach (var rb in rbs)
            {
                if (rb == null) continue;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.Sleep();
            }
        }
        catch (Exception ex)
        {
            SuperController.LogMessage("AtomBatch.ResetPhysics error: " + ex.Message);
        }
    }

    // Do a safe move for all Person atoms: disable collisions, move to unoccupied positions, reset physics, then reenable collisions optionally.
    public static void SafeMoveAllPersons(bool reenableCollision = true)
    {
        var persons = GetAllPersonAtoms();
        SuperController.LogMessage($"AtomBatch.SafeMoveAllPersons: found {persons.Count} person atoms");
        foreach (var p in persons)
        {
            try
            {
                DisableCollision(p);
                bool moved = MoveToUnoccupiedPosition(p);
                ResetPhysics(p);
                if (reenableCollision)
                {
                    EnableCollision(p);
                }
                SuperController.LogMessage($"AtomBatch: processed atom '{p.fullName}' moved={moved}");
            }
            catch (Exception ex)
            {
                SuperController.LogMessage("AtomBatch.SafeMoveAllPersons per-atom error: " + ex.Message);
            }
        }
    }

    // Convenience method to run from the in-game console: AtomBatch.SafeMoveAllPersons()
}
