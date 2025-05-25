# 🧬 CoreLib

**CoreLib** is the cryptographic core library of the [Arqanum](https://github.com/arqanumapp) project — a post-quantum, end-to-end encrypted messenger. 

- 🔐 **Post-quantum cryptography**
  - `ML-KEM-1024` (NIST PQC KEM) for asymmetric encryption (based on Kyber)
  - `ML-DSA (Dilithium5)` for digital signatures

- 🧾 **Serialization & Integrity**
  - Serialization of cryptographic containers and message envelopes
  - `SHAKE256` hash support for data integrity verification

- 🔑 **Symmetric encryption**
  - `AES-256-GCM` for secure data storage and transport


## 📦 Usage

- Secure message encryption and digital signing
- Creation and validation of cryptographic envelopes
- Key encapsulation and decapsulation
- Secure session key management

## 🔒 Security

- Implements NIST-approved post-quantum standards (finalists)
- Secure key handling with no leakage in logs or exceptions
- Message authentication and tamper protection

