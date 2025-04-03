# Job Profiling System

![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)
![C#](https://img.shields.io/badge/C%23-8.0-green)
![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2-purple)

## 📋 Overview

The Job Profiling System is a Windows Forms application for visualizing and comparing the performance of different sorting algorithms in real-time. This application incorporates principles of:

- **Job Scheduling**: Priority-based job queue management
- **Thread Affinity**: Optimizing processor core assignment
- **Algorithm Profiling**: Performance tracking and comparison
- **Concurrent Programming**: Multithreaded job execution

The system demonstrates how to implement a robust, thread-safe task scheduling framework with visualization components.

## 🌟 Features

- **Priority-Based Job Scheduling**: Jobs are queued and executed based on priority levels (High, Medium, Low)
- **Real-Time Performance Tracking**: Captures and displays execution times, maintaining averages
- **Thread Affinity Management**: Assigns worker threads to specific CPU cores
- **Algorithm Comparison**: Demonstrates performance differences between:
  - Bubble Sort (O(n²) complexity)
  - Quick Sort (O(n log n) complexity)
- **Job Management UI**: Cancel jobs, view execution status, track completed tasks
- **Event-Based Architecture**: Utilizes events for job lifecycle notifications
- **Thread-Safe Implementation**: Proper locking mechanisms for shared resources

## 🖥️ Screenshots

*(Screenshots would be inserted here)*

## 🚀 Getting Started

### Prerequisites

- Visual Studio 2019/2022 with .NET Desktop Development workload
- .NET Framework 4.7.2 or higher

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/Arundhuti2000/JobProfilingSystem.git
   cd JobProfilingSystem
   ```

2. **Open the solution**
   - Double-click `JobProfilingSystem.sln`
   - Or open via Visual Studio: File → Open → Project/Solution

3. **Build the solution**
   - Press F6 or select Build → Build Solution
   - Or via command line:
     ```bash
     msbuild JobProfilingSystem.sln /p:Configuration=Release
     ```

4. **Run the application**
   - Press F5 to run with debugging
   - Press Ctrl+F5 to run without debugging
   - Or navigate to `bin\Release` and run `JobProfilingSystem.exe`

## 📘 Usage Guide

### Adding Jobs

1. Enter a job name in the text box (supported types: "Bubble Sort" or "Quick Sort")
2. Select priority level from the dropdown
3. Click "Add Job" button

### Managing the Scheduler

- **Start Scheduler**: Begins processing jobs in the queue
- **Stop Scheduler**: Pauses job execution, preserving the queue
- **Cancel Jobs**: Right-click on a queued job to cancel it

### Monitoring Performance

- The list view displays job status, priority, and execution times
- The log window shows detailed algorithm operations

## 📐 Architecture

The system implements a multi-layered architecture:

### Core Components

1. **SortingJobSystem (Abstract Class)**
   - Base class for sorting algorithm implementations
   - Tracks execution time and performance metrics
   - Provides abstract `Execute()` method for algorithm implementation

2. **JobScheduler**
   - Priority queue management
   - Thread affinity allocation
   - Worker thread management
   - Event-based notifications

3. **Algorithm Implementations**
   - `BubbleSortJob`: Demonstrates O(n²) complexity
   - `QuickSortJob`: Demonstrates O(n log n) complexity

4. **UI Components**
   - Job submission interface
   - Real-time status monitoring
   - Execution logging

### Technical Details

- **Thread Affinity**: Uses P/Invoke to access Windows kernel32.dll for core allocation
- **Priority Queue**: Implemented with `SortedDictionary<int, Queue<T>>` for efficient priority management
- **Thread Safety**: Lock-based synchronization for queue access

## 🔧 Technical Skills Demonstrated

- **Multithreaded Programming**: Thread management, synchronization, affinity settings
- **Design Patterns**: Observer pattern (via events), Template Method pattern
- **Algorithm Implementation**: Sorting algorithms with different complexity classes
- **Windows Forms UI Development**: Custom controls, event handling, dynamic updates
- **Performance Profiling**: Runtime measurement, statistical tracking
- **P/Invoke**: Native Windows API integration for thread management
- **OOP Principles**: Inheritance, abstraction, encapsulation

## 🛠️ Future Enhancements

Potential areas for expansion:

- Additional sorting algorithms (Merge Sort, Heap Sort, etc.)
- Visual comparison charts for algorithm performance
- Customizable data set generation
- Job scheduling policy configuration
- Export performance data to CSV for analysis

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 👤 Author

Arundhati Das - [arundhutidas2000@gmail.com](mailto:arundhutidas2000@gmail.com)

---

*This project was developed as a demonstration of multithreaded programming and algorithm profiling techniques in C#.*
